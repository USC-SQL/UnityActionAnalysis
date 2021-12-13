using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class Compare : Operation
    {
        private Variable value1Var;
        private ComparisonKind op;
        private Variable value2Var;
        private Variable resultVar;

        public Compare(Variable value1Var, ComparisonKind op, Variable value2Var, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.value1Var = value1Var;
            this.op = op;
            this.value2Var = value2Var;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            Expr value1 = state.MemoryRead(value1Var.address, value1Var.type);
            Expr value2 = state.MemoryRead(value2Var.address, value2Var.type);
            Sort inputSort = SymexMachine.Instance.SortPool.TypeToSort(value1Var.type);
            BitVecSort resultSort = (BitVecSort)SymexMachine.Instance.SortPool.TypeToSort(resultVar.type);
            if (inputSort is IntSort)
            {
                // reference comparison
                Reference ref1 = Reference.FromExpr(value1);
                Reference ref2 = Reference.FromExpr(value2);
                bool compResult;
                switch (op)
                {
                    case ComparisonKind.Equality:
                        compResult = ref1.Equals(ref2);
                        break;
                    case ComparisonKind.Inequality:
                        compResult = !ref1.Equals(ref2);
                        break;
                    case ComparisonKind.GreaterThan:
                        Debug.Assert(ref2.address == null);
                        compResult = ref1.address != null && ref2.address == null;
                        break;
                    case ComparisonKind.LessThan:
                        Debug.Assert(ref1.address == null);
                        compResult = ref1.address == null && ref2.address != null;
                        break;
                    default:
                        throw new Exception("unexpected reference comparison kind " + op);
                }
                BoolExpr result = z3.MkBool(compResult);
                BitVecExpr bvResult = (BitVecExpr)z3.MkITE(result, z3.MkBV(1, resultSort.Size), z3.MkBV(0, resultSort.Size));
                state.MemoryWrite(resultVar.address, bvResult);
            } else
            {
                bool isBV = inputSort is BitVecSort;
                bool isUnsignedBV = isBV && !SymexMachine.Instance.SortPool.IsSigned(value1Var.type);
                BoolExpr result;
                switch (op)
                {
                    case ComparisonKind.Equality:
                        result = z3.MkEq(value1, value2);
                        break;
                    case ComparisonKind.Inequality:
                        result = z3.MkNot(z3.MkEq(value1, value2));
                        break;
                    case ComparisonKind.LessThan:
                        if (isBV)
                        {
                            if (isUnsignedBV)
                            {
                                result = z3.MkBVULT((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                            else
                            {
                                result = z3.MkBVSLT((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                        }
                        else
                        {
                            result = z3.MkAnd(
                                z3.MkFPLt((FPExpr)value1, (FPExpr)value2), 
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value1)), 
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value2)));
                        }
                        break;
                    case ComparisonKind.LessThanOrEqual:
                        if (isBV)
                        {
                            if (isUnsignedBV)
                            {
                                result = z3.MkBVULE((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                            else
                            {
                                result = z3.MkBVSLE((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                        }
                        else
                        {
                            result = z3.MkAnd(
                                z3.MkFPLEq((FPExpr)value1, (FPExpr)value2),
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value1)),
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value2)));
                        }
                        break;
                    case ComparisonKind.GreaterThan:
                        if (isBV)
                        {
                            if (isUnsignedBV)
                            {
                                result = z3.MkBVUGT((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                            else
                            {
                                result = z3.MkBVSGT((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                        }
                        else
                        {
                            result = z3.MkAnd(
                                z3.MkFPGt((FPExpr)value1, (FPExpr)value2),
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value1)),
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value2)));
                        }
                        break;
                    case ComparisonKind.GreaterThanOrEqual:
                        if (isBV)
                        {
                            if (isUnsignedBV)
                            {
                                result = z3.MkBVUGE((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                            else
                            {
                                result = z3.MkBVSGE((BitVecExpr)value1, (BitVecExpr)value2);
                            }
                        }
                        else
                        {
                            result = z3.MkAnd(
                                z3.MkFPGEq((FPExpr)value1, (FPExpr)value2),
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value1)),
                                z3.MkNot(z3.MkFPIsNaN((FPExpr)value2)));
                        }
                        break;
                    default:
                        throw new Exception("unknown comparison kind " + op);
                }

                BitVecExpr bvResult = (BitVecExpr)z3.MkITE(result, z3.MkBV(1, resultSort.Size), z3.MkBV(0, resultSort.Size));
                state.MemoryWrite(resultVar.address, bvResult);
            }
        }
    }
}