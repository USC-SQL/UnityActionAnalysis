using System;
using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{

    public class BinaryOp : Operation
    {
        private Variable value1Var;
        private BinaryNumericOperator op;
        private Variable value2Var;
        private Variable resultVar;

        public BinaryOp(Variable value1Var, BinaryNumericOperator op, Variable value2Var, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.value1Var = value1Var;
            this.op = op;
            this.value2Var = value2Var;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            Sort sort = SymexMachine.Instance.SortPool.TypeToSort(value1Var.type);
            Expr value1 = state.MemoryRead(value1Var.address, value1Var.type);
            Expr value2 = state.MemoryRead(value2Var.address, value2Var.type);
            bool isBV = sort is BitVecSort;
            bool isUnsignedBV = isBV && !SymexMachine.Instance.SortPool.IsSigned(value1Var.type);
            Expr result;
            switch (op)
            {
                case BinaryNumericOperator.Add:
                    if (isBV)
                    {
                        result = z3.MkBVAdd((BitVecExpr)value1, (BitVecExpr)value2);
                    } else
                    {
                        result = z3.MkFPAdd(z3.MkFPRTZ(), (FPExpr)value1, (FPExpr)value2);
                    }
                    break;
                case BinaryNumericOperator.Sub:
                    if (isBV)
                    {
                        result = z3.MkBVSub((BitVecExpr)value1, (BitVecExpr)value2);
                    }
                    else
                    {
                        result = z3.MkFPSub(z3.MkFPRTZ(), (FPExpr)value1, (FPExpr)value2);
                    }
                    break;
                case BinaryNumericOperator.Mul:
                    if (isBV)
                    {
                        result = z3.MkBVMul((BitVecExpr)value1, (BitVecExpr)value2);
                    }
                    else
                    {
                        result = z3.MkFPMul(z3.MkFPRTZ(), (FPExpr)value1, (FPExpr)value2);
                    }
                    break;
                case BinaryNumericOperator.Div:
                    if (isBV)
                    {
                        if (isUnsignedBV)
                        {
                            result = z3.MkBVUDiv((BitVecExpr)value1, (BitVecExpr)value2);
                        } else
                        {
                            result = z3.MkBVSDiv((BitVecExpr)value1, (BitVecExpr)value2);
                        }
                    }
                    else
                    {
                        result = z3.MkFPDiv(z3.MkFPRTZ(), (FPExpr)value1, (FPExpr)value2);
                    }
                    break;
                case BinaryNumericOperator.Rem:
                    if (isBV)
                    {
                        if (isUnsignedBV)
                        {
                            result = z3.MkBVURem((BitVecExpr)value1, (BitVecExpr)value2);
                        } else
                        {
                            result = z3.MkBVSRem((BitVecExpr)value1, (BitVecExpr)value2);
                        }
                    } else
                    {
                        result = z3.MkFPRem((FPExpr)value1, (FPExpr)value2);
                    }
                    break;
                case BinaryNumericOperator.BitAnd:
                    result = z3.MkBVAND((BitVecExpr)value1, (BitVecExpr)value2);
                    break;
                case BinaryNumericOperator.BitOr:
                    result = z3.MkBVOR((BitVecExpr)value1, (BitVecExpr)value2);
                    break;
                case BinaryNumericOperator.BitXor:
                    result = z3.MkBVXOR((BitVecExpr)value1, (BitVecExpr)value2);
                    break;
                case BinaryNumericOperator.ShiftLeft:
                    result = z3.MkBVSHL((BitVecExpr)value1, (BitVecExpr)value2);
                    break;
                case BinaryNumericOperator.ShiftRight:
                    result = z3.MkBVASHR((BitVecExpr)value1, (BitVecExpr)value2);
                    break;
                default:
                    throw new Exception("unrecognized operator " + op);
            }
            state.MemoryWrite(resultVar.address, result);
        }
    }

}