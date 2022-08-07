using Microsoft.Z3;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnityActionAnalysis.Operations
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
            Expr value = state.MemoryRead(valueVar.address, valueVar.type);
            state.MemoryWrite(destVar.address, value);
        }
    }
}
