using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeStaticFieldRef : Operation
    {
        private IField field;
        private Variable resultVar;

        public MakeStaticFieldRef(IField field, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.field = field;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            MemoryAddress address = new MemoryAddress(false, "staticfield:" + Helpers.GetAssemblyQualifiedName(field.DeclaringType) + "." + field.Name);
            Reference r = new Reference(field.Type, address);
            state.MemoryRead(r.address, r.type);
            state.MemoryWrite(resultVar.address, r.ToExpr());
        }
    }
}
