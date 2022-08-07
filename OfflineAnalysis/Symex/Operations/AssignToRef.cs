using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class AssignToRef : Operation
    {
        private Variable refVar;
        private Variable valueVar;

        public AssignToRef(Variable refVar, Variable valueVar, ILInstruction inst) : base(inst)
        {
            this.refVar = refVar;
            this.valueVar = valueVar;
        }

        public override void Perform(SymexState state)
        {
            Debug.Assert(refVar.IsReferenceType());
            Expr refExpr = state.MemoryRead(refVar.address, null);
            Reference r = Reference.FromExpr(refExpr);
            Expr value = state.MemoryRead(valueVar.address, valueVar.type);
            state.MemoryWrite(r.address, value);
        }
    }
}
