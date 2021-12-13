using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler.Operations
{
    public class Fetch : Operation
    {
        private InstructionPointer IP;
        public Fetch(InstructionPointer IP, ILInstruction inst) : base(inst) 
        {
            this.IP = IP;
        }

        private void Enqueue(SymexState s, Operation op)
        {
            s.opQueue.Enqueue(op);
        }

        private void EnqueueEvaluate(SymexState s, ILInstruction inst, Variable resultVar)
        {
            switch (inst.OpCode)
            {
                case OpCode.LdLoc:
                    {
                        LdLoc ldloc = (LdLoc)inst;
                        Variable destVar = new Variable(ldloc.Variable.Type);
                        Enqueue(s, new MakeLocalDestVar(destVar, ldloc.Variable, inst));
                        if (resultVar != null)
                        {
                            Enqueue(s, new Assign(resultVar, destVar, inst));
                        }
                    }
                    break;
                case OpCode.LdLoca:
                    {
                        LdLoca ldloca = (LdLoca)inst;
                        Variable destVar = new Variable(ldloca.Variable.Type);
                        Enqueue(s, new MakeLocalDestVar(destVar, ldloca.Variable, inst));
                        if (resultVar != null)
                        {
                            Enqueue(s, new MakeRef(destVar, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.LdFlda:
                    {
                        LdFlda ldflda = (LdFlda)inst;
                        Variable refVar = Variable.Reference();
                        Enqueue(s, new MakeTempVar(refVar, inst));
                        EnqueueEvaluate(s, ldflda.Target, refVar);
                        if (resultVar != null)
                        {
                            Enqueue(s, new MakeObjectFieldRef(refVar, ldflda.Field, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.LdsFlda:
                    {
                        LdsFlda ldsflda = (LdsFlda)inst;
                        if (resultVar != null)
                        {
                            Enqueue(s, new MakeStaticFieldRef(ldsflda.Field, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.LdElema:
                    {
                        LdElema ldelema = (LdElema)inst;
                        if (ldelema.Indices.Count > 1)
                        {
                            throw new NotSupportedException("multi-dimensional arrays not supported");
                        }
                        ILInstruction index = ldelema.Indices[0];
                        IType indexType = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Int32);
                        Variable refVar = Variable.Reference();
                        Variable indexVar = new Variable(indexType);
                        Enqueue(s, new MakeTempVar(refVar, inst));
                        Enqueue(s, new MakeTempVar(indexVar, inst));
                        EnqueueEvaluate(s, ldelema.Array, refVar);
                        EnqueueEvaluate(s, index, indexVar);
                        if (resultVar != null)
                        {
                            Enqueue(s, new MakeArrayElementRef(refVar, indexVar, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.LdObj:
                    {
                        LdObj ldobj = (LdObj)inst;
                        Variable refVar = Variable.Reference();
                        Enqueue(s, new MakeTempVar(refVar, inst));
                        EnqueueEvaluate(s, ldobj.Target, refVar);
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignFromRef(resultVar, refVar, inst));
                        }
                    }
                    break;
                case OpCode.LdLen:
                    {
                        LdLen ldlen = (LdLen)inst;
                        Variable refVar = Variable.Reference();
                        Enqueue(s, new MakeTempVar(refVar, inst));
                        EnqueueEvaluate(s, ldlen.Array, refVar);
                        if (resultVar != null)
                        {
                            Enqueue(s, new ReadArrayLength(refVar, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.LdcDecimal:
                    {
                        LdcDecimal ldcdecimal = (LdcDecimal)inst;
                        throw new NotSupportedException("decimal type not supported");
                    }
                case OpCode.LdcF4:
                    {
                        LdcF4 ldcf4 = (LdcF4)inst;
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignConstantFloat(resultVar, ldcf4.Value, inst));
                        }
                    }
                    break;
                case OpCode.LdcF8:
                    {
                        LdcF8 ldcf8 = (LdcF8)inst;
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignConstantDouble(resultVar, ldcf8.Value, inst));
                        }
                    }
                    break;
                case OpCode.LdcI4:
                    {
                        LdcI4 ldci4 = (LdcI4)inst;
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignConstantInt32(resultVar, ldci4.Value, inst));
                        }
                    }
                    break;
                case OpCode.LdcI8:
                    {
                        LdcI8 ldci8 = (LdcI8)inst;
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignConstantInt64(resultVar, ldci8.Value, inst));
                        }
                    }
                    break;
                case OpCode.LdStr:
                    {
                        LdStr ldcstr = (LdStr)inst;
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignConstantString(resultVar, ldcstr.Value, inst));
                        }
                    }
                    break;
                case OpCode.LdNull:
                    {
                        LdNull ldnull = (LdNull)inst;
                        if (ldnull != null)
                        {
                            Enqueue(s, new AssignNull(resultVar, inst));
                        }
                    }
                    break;
                case OpCode.LdMemberToken:
                    {
                        LdMemberToken ldmembertoken = (LdMemberToken)inst;
                        if (ldmembertoken != null)
                        {
                            Enqueue(s, new AssignConstantMemberToken(resultVar, ldmembertoken.Member, inst));
                        }
                    }
                    break;
                case OpCode.AddressOf:
                    {
                        AddressOf addressof = (AddressOf)inst;
                        ILInstruction value = addressof.Value;
                        Debug.Assert(value is LdLoc);
                        LdLoc localdest = (LdLoc)value;
                        Variable localdestVar = new Variable(localdest.Variable.Type);
                        Enqueue(s, new MakeLocalDestVar(localdestVar, localdest.Variable, inst));
                        if (resultVar != null)
                        {
                            Enqueue(s, new MakeRef(localdestVar, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.NewObj:
                    {
                        NewObj newobj = (NewObj)inst;
                        IMethod ctor = newobj.Method;
                        IType type = ctor.DeclaringType;
                        if (type.Kind == TypeKind.Class)
                        {
                            Variable thisVar = new Variable(type);
                            List<Variable> argVars = new List<Variable>();
                            for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                            {
                                IParameter param = ctor.Parameters[i];
                                argVars.Add(new Variable(param.Type));
                            }
                            if (SymexMachine.Instance.Config.IsMethodSymbolic(ctor))
                            {
                                Enqueue(s, new MakeTempVar(thisVar, inst));
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    Enqueue(s, new MakeTempVar(argVars[i], inst));
                                }
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    EnqueueEvaluate(s, newobj.Arguments[i], argVars[i]);
                                }
                                Enqueue(s, new AssignSymbolicConstructorResult(thisVar, ctor, argVars, inst));
                                if (resultVar != null)
                                {
                                    Enqueue(s, new Assign(resultVar, thisVar, inst));
                                }
                            } else
                            {
                                Enqueue(s, new MakeInvokeThisVar(thisVar, inst));
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    Enqueue(s, new MakeInvokeArgVar(argVars[i], i, inst));
                                }
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    EnqueueEvaluate(s, newobj.Arguments[i], argVars[i]);
                                }
                                Enqueue(s, new AssignNewClassInstance(thisVar, type, inst));
                                Enqueue(s, new Call(SymexMachine.Instance.MethodPool.MethodEntryPoint(ctor), inst));
                                if (resultVar != null)
                                {
                                    Enqueue(s, new Assign(resultVar, thisVar, inst));
                                }
                            }
                        } else if (type.Kind == TypeKind.Struct)
                        {
                            Variable objVar = new Variable(type);
                            List<Variable> argVars = new List<Variable>();
                            for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                            {
                                IParameter param = ctor.Parameters[i];
                                argVars.Add(new Variable(param.Type));
                            }

                            if (SymexMachine.Instance.Config.IsMethodSymbolic(ctor))
                            {
                                Enqueue(s, new MakeTempVar(objVar, inst));
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    Enqueue(s, new MakeTempVar(argVars[i], inst));
                                }
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    EnqueueEvaluate(s, newobj.Arguments[i], argVars[i]);
                                }
                                Enqueue(s, new AssignSymbolicConstructorResult(objVar, ctor, argVars, inst));
                                if (resultVar != null)
                                {
                                    Enqueue(s, new Assign(resultVar, objVar, inst));
                                }
                            } else
                            {
                                Variable thisVar = Variable.Reference();
                                Enqueue(s, new MakeTempVar(objVar, inst));
                                Enqueue(s, new MakeInvokeThisVar(thisVar, inst));
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    Enqueue(s, new MakeInvokeArgVar(argVars[i], i, inst));
                                }
                                for (int i = 0, n = ctor.Parameters.Count; i < n; ++i)
                                {
                                    EnqueueEvaluate(s, newobj.Arguments[i], argVars[i]);
                                }
                                Enqueue(s, new AssignDefaultValue(objVar, type, inst));
                                Enqueue(s, new MakeRef(objVar, thisVar, inst));
                                Enqueue(s, new Call(SymexMachine.Instance.MethodPool.MethodEntryPoint(ctor), inst));
                                if (resultVar != null)
                                {
                                    Enqueue(s, new Assign(resultVar, objVar, inst));
                                }
                            }
                        } else
                        {
                            throw new Exception("unexpected call to newobj with constructor " + ctor + " (kind " + type.Kind + ")");
                        }
                    }
                    break;
                case OpCode.NewArr:
                    {
                        NewArr newarr = (NewArr)inst;
                        if (newarr.Indices.Count > 1)
                        {
                            throw new NotSupportedException("multi-dimensional arrays not supported");
                        }
                        ILInstruction length = newarr.Indices[0];
                        IType lengthType = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Int32);
                        ArrayType arrType = new ArrayType(SymexMachine.Instance.CSD.TypeSystem, newarr.Type, 1);
                        Variable lenVar = new Variable(lengthType);
                        Enqueue(s, new MakeTempVar(lenVar, inst));
                        EnqueueEvaluate(s, length, lenVar);
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignNewArray(resultVar, arrType, lenVar, inst));
                        }
                    }
                    break;
                case OpCode.DefaultValue:
                    {
                        DefaultValue defaultvalue = (DefaultValue)inst;
                        if (resultVar != null)
                        {
                            Enqueue(s, new AssignDefaultValue(resultVar, defaultvalue.Type, inst));
                        }
                    }
                    break;
                case OpCode.Call:
                    {
                        ICSharpCode.Decompiler.IL.Call call = (ICSharpCode.Decompiler.IL.Call)inst;
                        IMethod method = call.Method;
                        List<Variable> argVars = new List<Variable>();
                        for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                        {
                            IParameter param = method.Parameters[i];
                            argVars.Add(new Variable(param.Type));
                        }
                        if (SymexMachine.Instance.Config.IsMethodSymbolic(method))
                        {
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                Enqueue(s, new MakeTempVar(argVars[i], inst));
                            }
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                EnqueueEvaluate(s, call.Arguments[i], argVars[i]);
                            }
                            if (resultVar != null)
                            {
                                Enqueue(s, new AssignSymbolicStaticMethodResult(resultVar, method, argVars, inst));
                            }
                        } else
                        {
                            Variable retVar = new Variable(method.ReturnType);
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                Enqueue(s, new MakeInvokeArgVar(argVars[i], i, inst));
                            }
                            Enqueue(s, new MakeInvokeReturnVar(retVar, inst));
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                EnqueueEvaluate(s, call.Arguments[i], argVars[i]);
                            }
                            Enqueue(s, new Call(SymexMachine.Instance.MethodPool.MethodEntryPoint(method), inst));
                            if (resultVar != null)
                            {
                                Enqueue(s, new Assign(resultVar, retVar, inst));
                            }
                        }
                    }
                    break;
                case OpCode.CallVirt:
                    {
                        CallVirt callvirt = (CallVirt)inst;
                        IMethod method = callvirt.Method;
                        IType thisType = method.DeclaringType;
                        Variable thisVar = new Variable(thisType);
                        List<Variable> argVars = new List<Variable>();
                        for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                        {
                            IParameter param = method.Parameters[i];
                            argVars.Add(new Variable(param.Type));
                        }
                        if (SymexMachine.Instance.Config.IsMethodSymbolic(method))
                        {
                            Enqueue(s, new MakeTempVar(thisVar, inst));
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                Enqueue(s, new MakeTempVar(argVars[i], inst));
                            }
                            EnqueueEvaluate(s, callvirt.Arguments[0], thisVar);
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                EnqueueEvaluate(s, callvirt.Arguments[i + 1], argVars[i]);
                            }
                            if (resultVar != null)
                            {
                                Enqueue(s, new AssignSymbolicInstanceMethodResult(resultVar, method, thisVar, argVars, inst));
                            }
                        } else
                        {
                            Variable retVar = new Variable(method.ReturnType);
                            Enqueue(s, new MakeInvokeThisVar(thisVar, inst));
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                Enqueue(s, new MakeInvokeArgVar(argVars[i], i, inst));
                            }
                            Enqueue(s, new MakeInvokeReturnVar(retVar, inst));
                            EnqueueEvaluate(s, callvirt.Arguments[0], thisVar);
                            for (int i = 0, n = method.Parameters.Count; i < n; ++i)
                            {
                                EnqueueEvaluate(s, callvirt.Arguments[i + 1], argVars[i]);
                            }
                            Enqueue(s, new Call(SymexMachine.Instance.MethodPool.MethodEntryPoint(method), inst));
                            if (resultVar != null)
                            {
                                Enqueue(s, new Assign(resultVar, retVar, inst));
                            }
                        }
                    }
                    break;
                case OpCode.Conv:
                    {
                        Conv conv = (Conv)inst;
                        IType typeFrom = SymexMachine.Instance.CSD.TypeSystem.FindType(TypeUtils.ToKnownTypeCode(conv.InputType, conv.InputSign));
                        IType typeTo = SymexMachine.Instance.CSD.TypeSystem.FindType(TypeUtils.ToKnownTypeCode(conv.TargetType));
                        Variable valueVar = new Variable(typeFrom);
                        Enqueue(s, new MakeTempVar(valueVar, inst));
                        EnqueueEvaluate(s, conv.Argument, valueVar);
                        if (resultVar != null)
                        {
                            Enqueue(s, new Convert(valueVar, typeTo, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.Comp:
                    {
                        Comp comp = (Comp)inst;
                        IType type;
                        if (comp.InputType == StackType.Ref)
                        {
                            type = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Object);
                        } else
                        {
                            type = SymexMachine.Instance.CSD.TypeSystem.FindType(TypeUtils.ToKnownTypeCode(comp.InputType, comp.Sign));
                        }
                        Variable value1Var = new Variable(type);
                        Variable value2Var = new Variable(type);
                        Enqueue(s, new MakeTempVar(value1Var, inst));
                        Enqueue(s, new MakeTempVar(value2Var, inst));
                        EnqueueEvaluate(s, comp.Left, value1Var);
                        EnqueueEvaluate(s, comp.Right, value2Var);
                        if (resultVar != null)
                        {
                            Enqueue(s, new Compare(value1Var, comp.Kind, value2Var, resultVar, inst));
                        }
                    }
                    break;
                case OpCode.BinaryNumericInstruction:
                    {
                        BinaryNumericInstruction binop = (BinaryNumericInstruction)inst;
                        IType type1 = SymexMachine.Instance.CSD.TypeSystem.FindType(TypeUtils.ToKnownTypeCode(binop.LeftInputType, binop.Sign));
                        IType type2 = SymexMachine.Instance.CSD.TypeSystem.FindType(TypeUtils.ToKnownTypeCode(binop.RightInputType, binop.Sign));
                        Variable value1Var = new Variable(type1);
                        Variable value2Var = new Variable(type2);
                        Enqueue(s, new MakeTempVar(value1Var, inst));
                        Enqueue(s, new MakeTempVar(value2Var, inst));
                        EnqueueEvaluate(s, binop.Left, value1Var);
                        EnqueueEvaluate(s, binop.Right, value2Var);
                        if (resultVar != null)
                        {
                            Enqueue(s, new BinaryOp(value1Var, binop.Operator, value2Var, resultVar, inst));
                        }
                    }
                    break;
                default:
                    throw new Exception("unrecognized instruction: " + inst);
            }
        }

        public override void Perform(SymexState s)
        {
            if (IP.index >= IP.block.Instructions.Count)
            {
                // reached end of program
                return;
            }

            ILInstruction inst = IP.block.Instructions[IP.index];
            switch (inst.OpCode)
            {
                case OpCode.StLoc:
                    {
                        StLoc stloc = (StLoc)inst;
                        Variable destVar = new Variable(stloc.Variable.Type);
                        Enqueue(s, new MakeLocalDestVar(destVar, stloc.Variable, inst));
                        EnqueueEvaluate(s, stloc.Value, destVar);
                        Enqueue(s, new Fetch(IP.NextInstruction(), inst));
                    }
                    break;
                case OpCode.StObj:
                    {
                        StObj stobj = (StObj)inst;
                        Variable refVar = Variable.Reference();
                        Variable valVar = new Variable(stobj.Type);
                        Enqueue(s, new MakeTempVar(refVar, inst));
                        Enqueue(s, new MakeTempVar(valVar, inst));
                        EnqueueEvaluate(s, stobj.Target, refVar);
                        EnqueueEvaluate(s, stobj.Value, valVar);
                        Enqueue(s, new AssignToRef(refVar, valVar, inst));
                        Enqueue(s, new Fetch(IP.NextInstruction(), inst));
                    }
                    break;
                case OpCode.Nop:
                    Enqueue(s, new Fetch(IP.NextInstruction(), inst));
                    break;
                case OpCode.Branch:
                    {
                        ICSharpCode.Decompiler.IL.Branch branch = (ICSharpCode.Decompiler.IL.Branch)inst;
                        InstructionPointer nextIP = new InstructionPointer(branch.TargetBlock, 0);
                        Enqueue(s, new Fetch(nextIP, inst));
                    }
                    break;
                case OpCode.IfInstruction:
                    {
                        IfInstruction ifinst = (IfInstruction)inst;
                        Debug.Assert(ifinst.FalseInst.OpCode == OpCode.Nop);
                        Debug.Assert(ifinst.TrueInst.OpCode == OpCode.Branch);
                        ICSharpCode.Decompiler.IL.Branch trueBranch = (ICSharpCode.Decompiler.IL.Branch)ifinst.TrueInst;
                        IType boolType = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Boolean);
                        Variable condVar = new Variable(boolType);
                        Variable invCondVar = new Variable(boolType);
                        Enqueue(s, new MakeTempVar(condVar, inst));
                        Enqueue(s, new MakeTempVar(invCondVar, inst));
                        EnqueueEvaluate(s, ifinst.Condition, condVar);
                        Enqueue(s, new BoolNot(condVar, invCondVar, inst));
                        Enqueue(s, new Branch(new List<BranchCase>()
                        {
                            new BranchCase(condVar, new InstructionPointer(trueBranch.TargetBlock, 0)),
                            new BranchCase(invCondVar, IP.NextInstruction())
                        }, inst));
                    }
                    break;
                case OpCode.SwitchInstruction:
                    {
                        SwitchInstruction swinst = (SwitchInstruction)inst;
                        IType boolType = SymexMachine.Instance.CSD.TypeSystem.FindType(KnownTypeCode.Boolean);
                        Variable valVar = new Variable(boolType);
                        List<Variable> condVars = new List<Variable>();
                        foreach (SwitchSection section in swinst.Sections)
                        {
                            condVars.Add(new Variable(boolType));
                        }
                        Enqueue(s, new MakeTempVar(valVar, inst));
                        foreach (Variable condVar in condVars)
                        {
                            Enqueue(s, new MakeTempVar(condVar, inst));
                        }
                        EnqueueEvaluate(s, swinst.Value, valVar);
                        for (int i = 0, n = swinst.Sections.Count; i < n; ++i)
                        {
                            SwitchSection section = swinst.Sections[i];
                            Variable condVar = condVars[i];
                            if (section.HasNullLabel)
                            {
                                throw new NotImplementedException("support for switch sections with HasNullLabel not implemented");
                            }
                            Enqueue(s, new ValueInLongSet(valVar, section.Labels, condVar, inst));
                        }
                        List<BranchCase> branchCases = new List<BranchCase>();
                        for (int i = 0, n = swinst.Sections.Count; i < n; ++i)
                        {
                            SwitchSection section = swinst.Sections[i];
                            Variable condVar = condVars[i];
                            ICSharpCode.Decompiler.IL.Branch br = (ICSharpCode.Decompiler.IL.Branch)section.Body;
                            branchCases.Add(new BranchCase(condVar, new InstructionPointer(br.TargetBlock, 0)));
                        }
                        Enqueue(s, new Branch(branchCases, inst));
                    }
                    break;
                case OpCode.Leave:
                    {
                        Leave leave = (Leave)inst;
                        ILInstruction value = leave.Value;
                        if (value is Nop)
                        {
                            Enqueue(s, new Return(inst));
                        } else
                        {
                            IMethod currentMethod = IP.GetCurrentMethod();
                            Variable retVar = new Variable(currentMethod.ReturnType);
                            Enqueue(s, new MakeReturnVar(retVar, inst));
                            EnqueueEvaluate(s, value, retVar);
                            Enqueue(s, new Return(inst));
                        }
                    }
                    break;
                case OpCode.Throw:
                    Enqueue(s, new Abort(inst));
                    break;
                default:
                    EnqueueEvaluate(s, inst, null);
                    Enqueue(s, new Fetch(IP.NextInstruction(), inst));
                    break;
            }
        }
    }
}
