using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.IL.ControlFlow;
using ICSharpCode.Decompiler.FlowAnalysis;

namespace UnitySymexCrawler
{
    public class InputBranchAnalysis
    {
        private IMethod entryPoint;
        public readonly MethodPool pool;
        private Dictionary<IMethod, ControlFlowGraph> cfgs;

        public InputBranchAnalysis(IMethod entryPoint, MethodPool pool)
        {
            this.entryPoint = entryPoint;
            this.pool = pool;
            cfgs = new Dictionary<IMethod, ControlFlowGraph>();
        }

        private ILFunction FetchMethod(IMethod m)
        {
            return (ILFunction)pool.MethodEntryPoint(m).block.Parent.Parent;
        }

        private bool IsCandidate(IMethod m)
        {
            if (!m.HasBody || UnityConfiguration.IsInputAPI(m))
            {
                return false;
            }
            return m.ParentModule == entryPoint.ParentModule;
        }

        private IEnumerable<IMethod> CheckCallInstruction(ILInstruction inst, ISet<IMethod> visited)
        {
            if (FindCallInstruction(inst, out CallInstruction callInst))
            {
                IMethod target = callInst.Method;
                if (!visited.Contains(target) && IsCandidate(target))
                {
                    foreach (IMethod m in DoFindMethods(target, visited))
                    {
                        yield return m;
                    }
                }
            } else
            {
                foreach (ILInstruction child in inst.Children)
                {
                    foreach (IMethod m in CheckCallInstruction(child, visited))
                    {
                        yield return m;
                    }
                }
            }
        }

        private IEnumerable<IMethod> DoFindMethods(IMethod m, ISet<IMethod> visited)
        {
            yield return m;
            visited.Add(m);
            ILFunction func = FetchMethod(m);
            BlockContainer bc = (BlockContainer)func.Body;
            foreach (Block b in bc.Blocks)
            {
                foreach (ILInstruction inst in b.Instructions)
                {
                    foreach (IMethod method in CheckCallInstruction(inst, visited))
                    {
                        yield return method;
                    }
                }
            }
        }

        private IEnumerable<IMethod> FindMethods(IMethod entryPoint)
        {
            HashSet<IMethod> visited = new HashSet<IMethod>();
            foreach (IMethod m in DoFindMethods(entryPoint, visited))
            {
                yield return m;
            }
        }

        public class InputFlowNodeState
        {
            public ISet<ILVariable> variables;
            public ISet<IField> fields;

            public InputFlowNodeState()
            {
                variables = new HashSet<ILVariable>();
                fields = new HashSet<IField>();
            }

            public InputFlowNodeState(InputFlowNodeState o)
            {
                variables = new HashSet<ILVariable>(o.variables);
                fields = new HashSet<IField>(o.fields);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is InputFlowNodeState))
                {
                    return false;
                }

                InputFlowNodeState o = (InputFlowNodeState)obj;
                return variables.SetEquals(o.variables) && fields.SetEquals(o.fields);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(variables, fields);
            }

            public void Clear()
            {
                variables.Clear();
                fields.Clear();
            }

            public void AddAll(InputFlowNodeState o)
            {
                foreach (ILVariable v in o.variables)
                {
                    variables.Add(v);
                }

                foreach (IField f in o.fields)
                {
                    fields.Add(f);
                }
            }

            public override string ToString()
            {
                return "{" + string.Join(",", variables) + (fields.Count > 0 ? "," + string.Join(",", fields) : "") + "}";
            }
        }

        private class InputFlowAnalysisState
        {
            public Dictionary<ILInstruction, InputFlowNodeState> instIn;
            public Dictionary<ILInstruction, InputFlowNodeState> instOut;
            public bool mayReturnInput;
        }

        private class LeadsToAnalysisState
        {
            public Dictionary<ILInstruction, bool> instIn;
            public Dictionary<ILInstruction, bool> instOut;
            public bool entryPointLeads;
        }

        private class MethodAnalysisState
        {
            public ILFunction func;
            public ISet<Leave> leaveInstructions;
            public ControlFlowGraph cfg;
            public InputFlowAnalysisState inputFlow;
            public ISet<ILInstruction> inputDepBranchPoints;
            public LeadsToAnalysisState leadsTo;
        }

        public class MethodAnalysisResult
        {
            public ISet<ILInstruction> inputDepBranchPoints;
            public ISet<ILInstruction> leadsToInputDepBranchPoint;
            public bool mayReturnInput;
        }

        private void InitInputFlowAnalysis(MethodAnalysisState ms)
        {
            InputFlowAnalysisState s = ms.inputFlow = new InputFlowAnalysisState();
            BlockContainer bc = (BlockContainer)ms.func.Body;
            s.instIn = new Dictionary<ILInstruction, InputFlowNodeState>();
            s.instOut = new Dictionary<ILInstruction, InputFlowNodeState>();
            foreach (Block b in bc.Blocks)
            {
                foreach (ILInstruction inst in b.Instructions)
                {
                    s.instIn.Add(inst, new InputFlowNodeState());
                    s.instOut.Add(inst, new InputFlowNodeState());
                }
            }
            s.mayReturnInput = false;
        }

        private static IEnumerable<ILInstruction> Predecessors(ILInstruction inst, ControlFlowGraph cfg)
        {
            Block b = (Block)inst.Parent;
            if (inst.ChildIndex > 0)
            {
                yield return b.Instructions[inst.ChildIndex - 1];
            } else
            {
                ControlFlowNode cfgNode = cfg.GetNode(b);
                foreach (ControlFlowNode p in cfgNode.Predecessors)
                {
                    Block predBlock = (Block)p.UserData;
                    yield return predBlock.Instructions.Last();
                }
            }
        }

        private static IEnumerable<ILInstruction> Successors(ILInstruction inst, ControlFlowGraph cfg)
        {
            Block b = (Block)inst.Parent;
            if (inst.ChildIndex < b.Instructions.Count - 1)
            {
                yield return b.Instructions[inst.ChildIndex + 1];
            }
            else
            {
                ControlFlowNode cfgNode = cfg.GetNode(b);
                foreach (ControlFlowNode succ in cfgNode.Successors)
                {
                    Block succBlock = (Block)succ.UserData;
                    yield return succBlock.Instructions.First();
                }
            }
        }

        private static bool ExpressionContainsAnyOf(ILInstruction val, ISet<ILVariable> variables)
        {
            if (val is LdLoc ldloc)
            {
                return variables.Contains(ldloc.Variable);
            } else if (val is LdLoca ldloca)
            {
                return variables.Contains(ldloca.Variable);
            } else if (val is AddressOf addressof)
            {
                return ExpressionContainsAnyOf(addressof.Value, variables);
            } else if (val is CallInstruction call)
            {
                foreach (ILInstruction arg in call.Arguments)
                {
                    if (ExpressionContainsAnyOf(arg, variables))
                    {
                        return true;
                    }
                }
                return false;
            } else if (val is Conv conv)
            {
                return ExpressionContainsAnyOf(conv.Argument, variables);
            } else if (val is Comp comp)
            {
                return ExpressionContainsAnyOf(comp.Left, variables) || ExpressionContainsAnyOf(comp.Right, variables);
            } else if (val is BinaryNumericInstruction binop)
            {
                return ExpressionContainsAnyOf(binop.Left, variables) || ExpressionContainsAnyOf(binop.Right, variables);
            } else
            {
                return false;
            }
        }

        private static bool IsInputValue(ILInstruction val, InputFlowNodeState instIn, Dictionary<IMethod, MethodAnalysisState> methodStates)
        {
            return (val is CallInstruction call && (UnityConfiguration.IsInputAPI(call.Method) || (methodStates.TryGetValue(call.Method, out var s) && s.inputFlow.mayReturnInput)))
                || ((val is LdObj ldobj) && (ldobj.Target is IInstructionWithFieldOperand fop) && instIn.fields.Contains(fop.Field))
                || ExpressionContainsAnyOf(val, instIn.variables);
        }

        private static bool FindCallInstruction(ILInstruction inst, out CallInstruction result)
        {
            if (inst is CallInstruction callinst)
            {
                result = callinst;
                return true;
            } else if (inst is StLoc stloc)
            {
                return FindCallInstruction(stloc.Value, out result);
            } else
            {
                result = null;
                return false;
            }
        }

        private bool RunInputFlowAnalysis(MethodAnalysisState ms, Dictionary<IMethod, MethodAnalysisState> methodStates)
        {
            bool changed = false;
            InputFlowAnalysisState s = ms.inputFlow;
            ISet<ILInstruction> workList = new HashSet<ILInstruction>();
            BlockContainer bc = (BlockContainer)ms.func.Body;
            foreach (Block b in bc.Blocks)
            {
                foreach (ILInstruction inst in b.Instructions)
                {
                    workList.Add(inst);
                }
            }
            while (workList.Count > 0)
            {
                ILInstruction inst = workList.First();
                workList.Remove(inst);

                // in[n] = union of pred p out[p]
                InputFlowNodeState instIn = s.instIn[inst];
                instIn.Clear();
                foreach (ILInstruction pred in Predecessors(inst, ms.cfg))
                {
                    instIn.AddAll(s.instOut[pred]);
                }

                // transfer function
                InputFlowNodeState newOut = new InputFlowNodeState(instIn);
                if (inst is StLoc stloc)
                {
                    ILInstruction val = stloc.Value;
                    if (IsInputValue(val, instIn, methodStates))
                    {
                        newOut.variables.Add(stloc.Variable);
                    } else
                    {
                        newOut.variables.Remove(stloc.Variable);
                    }
                } else if (inst is StObj stobj && (stobj.Target is IInstructionWithFieldOperand fop))
                {
                    ILInstruction val = stobj.Value;
                    if (IsInputValue(val, instIn, methodStates))
                    {
                        newOut.fields.Add(fop.Field);
                    } else
                    {
                        newOut.fields.Remove(fop.Field);
                    }
                }
                if (!newOut.Equals(s.instOut[inst]))
                {
                    s.instOut[inst] = newOut;
                    changed = true;
                    foreach (ILInstruction succ in Successors(inst, ms.cfg))
                    {
                        workList.Add(succ);
                    }
                }
            }
            s.mayReturnInput = false;
            foreach (Leave leave in ms.leaveInstructions)
            {
                InputFlowNodeState instIn = s.instIn[leave];
                if (IsInputValue(leave.Value, instIn, methodStates))
                {
                    s.mayReturnInput = true;
                    break;
                }
            }
            return changed;
        }

        private void InitLeadsToAnalysis(MethodAnalysisState ms)
        {
            LeadsToAnalysisState s = ms.leadsTo = new LeadsToAnalysisState();
            BlockContainer bc = (BlockContainer)ms.func.Body;
            s.instIn = new Dictionary<ILInstruction, bool>();
            s.instOut = new Dictionary<ILInstruction, bool>();
            foreach (Block b in bc.Blocks)
            {
                foreach (ILInstruction inst in b.Instructions)
                {
                    s.instIn.Add(inst, false);
                    s.instOut.Add(inst, false);
                }
            }
            s.entryPointLeads = false;
        }

        private bool RunLeadsToAnalysis(MethodAnalysisState ms, Dictionary<IMethod, MethodAnalysisState> methodStates)
        {
            bool changed = false;
            LeadsToAnalysisState s = ms.leadsTo;
            ISet<ILInstruction> workList = new HashSet<ILInstruction>();
            BlockContainer bc = (BlockContainer)ms.func.Body;
            foreach (Block b in bc.Blocks)
            {
                foreach (ILInstruction inst in b.Instructions)
                {
                    workList.Add(inst);
                }
            }
            while (workList.Count > 0)
            {
                ILInstruction inst = workList.First();
                workList.Remove(inst);

                // out[n] = union of succ s in[s]
                bool instOut = false;
                foreach (ILInstruction succ in Successors(inst, ms.cfg))
                {
                    if (s.instIn[succ])
                    {
                        instOut = true;
                        break;
                    }
                }
                s.instOut[inst] = instOut;

                // transfer function
                bool b = instOut;
                if (ms.inputDepBranchPoints.Contains(inst))
                {
                    b = true;
                } else if (FindCallInstruction(inst, out CallInstruction callInst) && methodStates.TryGetValue(callInst.Method, out MethodAnalysisState ts) && ts.leadsTo.entryPointLeads)
                {
                    b = true;
                }

                if (b != s.instIn[inst])
                {
                    s.instIn[inst] = b;
                    changed = true;
                    foreach (ILInstruction pred in Predecessors(inst, ms.cfg))
                    {
                        workList.Add(pred);
                    }
                }
            }
            ILInstruction entry = bc.Blocks[0].Instructions.First();
            ms.leadsTo.entryPointLeads = s.instIn[entry];
            return changed;
        }

        public class Result
        {
            public Dictionary<IMethod, MethodAnalysisResult> methodResults;
        }

        public Result Perform()
        {
            ISet<IMethod> methods = new HashSet<IMethod>(FindMethods(entryPoint)); // methods reachable from entry point
            Dictionary<IMethod, MethodAnalysisState> methodStates = new Dictionary<IMethod, MethodAnalysisState>();
            foreach (IMethod m in methods)
            {
                MethodAnalysisState s = new MethodAnalysisState();
                s.func = FetchMethod(m);
                BlockContainer bc = (BlockContainer)s.func.Body;
                s.cfg = new ControlFlowGraph(bc);
                s.leaveInstructions = new HashSet<Leave>();
                foreach (Block b in bc.Blocks)
                {
                    foreach (ILInstruction inst in b.Instructions)
                    {
                        if (inst is Leave leave)
                        {
                            s.leaveInstructions.Add(leave);
                        }
                    }
                }
                methodStates.Add(m, s);
            }
            foreach (var p in methodStates)
            {
                InitInputFlowAnalysis(p.Value);
            }

            bool anyChanged = true;
            while (anyChanged)
            {
                anyChanged = false;
                foreach (var p in methodStates)
                {
                    if (RunInputFlowAnalysis(p.Value, methodStates))
                    {
                        anyChanged = true;
                    }
                }
            }

            foreach (var p in methodStates)
            {
                MethodAnalysisState s = p.Value;
                s.inputDepBranchPoints = new HashSet<ILInstruction>();
                BlockContainer bc = (BlockContainer)s.func.Body;
                foreach (Block b in bc.Blocks)
                {
                    foreach (ILInstruction inst in b.Instructions)
                    {
                        InputFlowNodeState instIn = s.inputFlow.instIn[inst];
                        if (inst is IfInstruction ifinst)
                        {
                            if (ExpressionContainsAnyOf(ifinst.Condition, instIn.variables))
                            {
                                s.inputDepBranchPoints.Add(inst);
                            }
                        }
                        else if (inst is SwitchInstruction swinst)
                        {
                            if (ExpressionContainsAnyOf(swinst.Value, instIn.variables))
                            {
                                s.inputDepBranchPoints.Add(inst);
                            }
                        }
                    }
                }
            }

            foreach (var p in methodStates)
            {
                MethodAnalysisState s = p.Value;
                InitLeadsToAnalysis(s);
            }

            anyChanged = true;
            while (anyChanged)
            {
                anyChanged = false;
                foreach (var p in methodStates)
                {
                    if (RunLeadsToAnalysis(p.Value, methodStates))
                    {
                        anyChanged = true;
                    }
                }
            }

            Result result = new Result();
            result.methodResults = new Dictionary<IMethod, MethodAnalysisResult>();
            foreach (var p in methodStates)
            {
                var s = p.Value;
                MethodAnalysisResult res = new MethodAnalysisResult();
                res.inputDepBranchPoints = s.inputDepBranchPoints;
                res.leadsToInputDepBranchPoint = new HashSet<ILInstruction>();
                foreach (var ap in s.leadsTo.instOut)
                {
                    if (ap.Value)
                    {
                        res.leadsToInputDepBranchPoint.Add(ap.Key);
                    }
                }
                res.mayReturnInput = s.inputFlow.mayReturnInput;
                result.methodResults.Add(p.Key, res);
            }
            return result;
        }
    }
}
