using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Util;

namespace UnityActionAnalysis.Operations
{
    public class ValueInLongSet : Operation
    {
        private Variable valueVar;
        private LongSet longSet;
        private Variable resultVar;

        public ValueInLongSet(Variable valueVar, LongSet longSet, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.valueVar = valueVar;
            this.longSet = longSet;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Debug.Assert(valueVar.type.FullName == "System.Int64");
            Context z3 = SymexMachine.Instance.Z3;
            BitVecExpr value = (BitVecExpr)state.MemoryRead(valueVar.address, valueVar.type);
            BitVecSort resultSort = (BitVecSort)SymexMachine.Instance.SortPool.TypeToSort(resultVar.type);
            List<BoolExpr> conditions = new List<BoolExpr>();
            foreach (LongInterval ivl in longSet.Intervals)
            {
                BitVecExpr start = z3.MkBV(ivl.Start, 64);
                BitVecExpr endInclusive = z3.MkBV(ivl.InclusiveEnd, 64);
                BoolExpr condition = z3.MkAnd(z3.MkBVSGE(value, start), z3.MkBVSLE(value, endInclusive));
                conditions.Add(condition);
            }
            BoolExpr result = z3.MkOr(conditions);
            BitVecExpr bvResult = (BitVecExpr)z3.MkITE(result, z3.MkBV(1, resultSort.Size), z3.MkBV(0, resultSort.Size));
            state.MemoryWrite(resultVar.address, bvResult);
        }
    }
}
