using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class AssignNewClassInstance : Operation
    {
        private Variable destVar;
        private IType type;

        public AssignNewClassInstance(Variable destVar, IType type, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.type = type;
        }

        public override void Perform(SymexState state)
        {
            MemoryAddress address = state.HeapAllocate(type.FullName);
            foreach (IField field in Helpers.GetInstanceFields(type))
            {
                MemoryAddress fieldAddress = new MemoryAddress(address.heap, address.root, new List<MemoryAddressComponent>() { new MemoryAddressField(field) });
                state.MemoryWrite(fieldAddress, Helpers.MakeDefaultValue(field.Type));
            }
            Reference r = new Reference(type, address);
            state.MemoryWrite(destVar.address, r.ToExpr());
        }
    }
}
