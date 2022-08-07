using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class AssignNewArray : Operation
    {
        private Variable destVar;
        private ArrayType type;
        private Variable lengthVar;

        public AssignNewArray(Variable destVar, ArrayType type, Variable lengthVar, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.type = type;
            this.lengthVar = lengthVar;
        }

        public override void Perform(SymexState state)
        {
            Context z3 = SymexMachine.Instance.Z3;
            MemoryAddress address = state.HeapAllocate(type, type.FullName);
            MemoryAddress elemsAddress = address.WithComponent(new MemoryAddressArrayElements());
            MemoryAddress lenAddress = address.WithComponent(new MemoryAddressArrayLength());
            Expr defaultElemValue = Helpers.MakeDefaultValue(type.ElementType);
            Expr length = state.MemoryRead(lengthVar.address, lengthVar.type);
            state.MemoryWrite(elemsAddress, z3.MkConstArray(z3.MkBitVecSort(32), defaultElemValue));
            state.MemoryWrite(lenAddress, length);
            Reference r = new Reference(type, address);
            state.MemoryWrite(destVar.address, r.ToExpr());
        }
    }
}
