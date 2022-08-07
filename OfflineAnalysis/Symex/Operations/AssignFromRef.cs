using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class AssignFromRef : Operation
    {
        private Variable destVar;
        private Variable refVar;

        public AssignFromRef(Variable destVar, Variable refVar, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.refVar = refVar;
        }

        public override void Perform(SymexState state)
        {
            Debug.Assert(refVar.IsReferenceType());
            Expr refExpr = state.MemoryRead(refVar.address, null);
            Reference r = Reference.FromExpr(refExpr);
            Expr value = state.MemoryRead(r.address, r.type);
            state.MemoryWrite(destVar.address, value);
        }
    }
}
