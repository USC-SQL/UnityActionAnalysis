using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeObjectFieldRef : Operation
    {
        private Variable refVar;
        private IField field;
        private Variable resultVar;

        public MakeObjectFieldRef(Variable refVar, IField field, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.refVar = refVar;
            this.field = field;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Debug.Assert(refVar.IsReferenceType());
            Reference r = Reference.FromExpr(state.MemoryRead(refVar.address, null));
            if (r.address == null)
            {
                new Abort(Instruction).Perform(state); // null pointer exception
                return;
            }
            MemoryAddress address = r.address.WithComponent(new MemoryAddressField(field));
            Reference res = new Reference(field.Type, address);
            state.MemoryRead(res.address, res.type); 
            state.MemoryWrite(resultVar.address, res.ToExpr());
        }
    }
}
