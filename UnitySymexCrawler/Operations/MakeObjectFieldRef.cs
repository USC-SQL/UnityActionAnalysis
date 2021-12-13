using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler.Operations
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
            List<MemoryAddressComponent> components = new List<MemoryAddressComponent>(r.address.components.Count + 1);
            foreach (MemoryAddressComponent c in r.address.components)
            {
                components.Add(c);
            }
            components.Add(new MemoryAddressField(field));
            MemoryAddress address = new MemoryAddress(r.address.heap, r.address.root, components);
            Reference res = new Reference(field.Type, address);
            state.MemoryRead(res.address, res.type); 
            state.MemoryWrite(resultVar.address, res.ToExpr());
        }
    }
}
