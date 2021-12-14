using Microsoft.Z3;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexCrawler.Operations
{
    public class Assign : Operation
    {
        private Variable destVar;
        private Variable valueVar;

        public Assign(Variable destVar, Variable valueVar, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.valueVar = valueVar;
        }

        public override void Perform(SymexState state)
        {
            if (!destVar.IsReferenceType() && !valueVar.IsReferenceType() && destVar.type.Kind == TypeKind.Struct && valueVar.type.Kind == TypeKind.Struct)
            {
                Sort destSort = SymexMachine.Instance.SortPool.TypeToSort(destVar.type);
                Sort valueSort = SymexMachine.Instance.SortPool.TypeToSort(valueVar.type);
                if (valueSort is BitVecSort && destSort is BitVecSort)
                {
                    BitVecSort destBvSort = (BitVecSort)destSort;
                    BitVecSort valueBvSort = (BitVecSort)valueSort;
                    if (destBvSort.Size != valueBvSort.Size)
                    {
                        // implicit bit-vector conversion
                        Convert conv = new Convert(valueVar, destVar.type, destVar, Instruction);
                        conv.Perform(state);
                        return;
                    }
                }
            }
            Expr value = state.MemoryRead(valueVar.address, valueVar.type);
            state.MemoryWrite(destVar.address, value);
        }
    }
}
