using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;
using UnitySymexCrawler.Operations;

namespace UnitySymexCrawler
{
    class ConfigData
    {
        public Dictionary<(string, string), int> symcallIds;

        public ConfigData()
        {
            symcallIds = new Dictionary<(string, string), int>();
        }

        public ConfigData(ConfigData o)
        {
            symcallIds = new Dictionary<(string, string), int>(o.symcallIds);
        }
    }

    public class UnityConfiguration : Configuration
    {
        private InputBranchAnalysis.Result ibaResult;

        public UnityConfiguration(InputBranchAnalysis.Result ibaResult)
        {
            this.ibaResult = ibaResult;
        }

        public override bool IsMethodSummarized(IMethod method)
        {
            if (!method.HasBody || IsInputAPI(method))
            {
                return true;
            }
            if (method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule)
            {
                return true;
            }
            ILInstruction entryPoint = SymexMachine.Instance.MethodPool.MethodEntryPoint(method).GetInstruction();
            InputBranchAnalysis.MethodAnalysisResult res = ibaResult.methodResults[method];
            return !res.mayReturnInput && !res.leadsToInputDepBranchPoint.Contains(entryPoint);
        }

        public override void ApplyMethodSummary(IMethod method, List<Expr> arguments, Variable resultVar, SymexState state)
        {
            ConfigData cdata = (ConfigData)state.customData;
            string args =
                string.Join(";", arguments.Select(arg => JsonSerializer.Serialize(state.SerializeExpr(arg))));
            var p = (method.ReflectionName, args);
            int symId;
            bool firstCall = false;
            if (!cdata.symcallIds.TryGetValue(p, out symId))
            {
                symId = state.symcallCounter++;
                cdata.symcallIds.Add(p, symId);
                firstCall = true;
            }
            Expr result = ApplySymcallMethodSummary(symId, method, arguments, resultVar, state);
            Context z3 = SymexMachine.Instance.Z3;
            if (firstCall && IsInputAPI(method))
            {
                switch (method.Name)
                {
                    case "GetAxis":
                    case "GetAxisRaw":
                        {
                            var resultSort = (FPSort)result.Sort;
                            var zero = z3.MkFPZero(resultSort, false);
                            SymexState forkPos = state.Fork();
                            SymexState forkNeg = state.Fork();
                            forkPos.pathCondition.Add(z3.MkFPGt((FPExpr)result, zero));
                            forkNeg.pathCondition.Add(z3.MkFPLt((FPExpr)result, zero));
                            state.pathCondition.Add(z3.MkFPEq((FPExpr)result, zero)); 
                        }
                        break;
                    case "GetButton":
                    case "GetButtonDown":
                    case "GetButtonUp":
                    case "GetKey":
                    case "GetKeyDown":
                    case "GetKeyUp":
                    case "GetMouseButton":
                    case "GetMouseButtonDown":
                    case "GetMouseButtonUp":
                        {
                            var resultSort = (BitVecSort)result.Sort;
                            SymexState fork = state.Fork();
                            fork.pathCondition.Add(z3.MkEq(result, z3.MkBV(0, resultSort.Size)));
                            state.pathCondition.Add(z3.MkEq(result, z3.MkBV(1, resultSort.Size)));
                            break;
                        }
                }
            }
        }

        public static bool IsInputAPI(IMethod method)
        {
            return method.DeclaringType.FullName == "UnityEngine.Input" && method.Parameters.Count == 1;
        }

        public override bool ShouldSkipBranchCase(BranchCase branchCase, ILInstruction branchInst, SymexState state)
        {
            IMethod method = branchCase.IP.GetCurrentMethod();
            InputBranchAnalysis.MethodAnalysisResult res = ibaResult.methodResults[method];
            if (res.inputDepBranchPoints.Contains(branchInst))
            {
                return false;
            }
            if (state.frameStack.Count > 0)
            {
                foreach (FrameStackElement fse in state.frameStack)
                {
                    var callInst = fse.opQueue.Peek().Instruction;
                    var m = Helpers.GetInstructionFunction(callInst).Method;
                    var mRes = ibaResult.methodResults[m];
                    if (mRes.leadsToInputDepBranchPoint.Contains(callInst))
                    {
                        return false;
                    }
                }
            }
            ILInstruction target = branchCase.IP.GetInstruction();
            return !res.leadsToInputDepBranchPoint.Contains(target);
        }

        public override object NewStateCustomData()
        {
            return new ConfigData();
        }

        public override object CloneStateCustomData(object data)
        {
            return new ConfigData((ConfigData)data);
        }
    }
}