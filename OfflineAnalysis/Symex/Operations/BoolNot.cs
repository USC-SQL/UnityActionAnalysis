using System;
using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class BoolNot : Operation
    {
        private Variable valueVar;
        private Variable resultVar;

        public BoolNot(Variable valueVar, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.valueVar = valueVar;
            this.resultVar = resultVar;
        }
        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            BitVecSort resultSort = (BitVecSort)SymexMachine.Instance.SortPool.TypeToSort(resultVar.type);
            BitVecExpr value = (BitVecExpr)state.MemoryRead(valueVar.address, valueVar.type);
            state.MemoryWrite(resultVar.address,
                z3.MkITE(
                    z3.MkEq(value, z3.MkBV(0, value.SortSize)),
                    z3.MkBV(1, resultSort.Size),
                    z3.MkBV(0, resultSort.Size)));
        }
    }
}
