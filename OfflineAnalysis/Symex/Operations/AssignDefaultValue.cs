using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class AssignDefaultValue : Operation
    {
        private Variable destVar;
        private IType type;

        public AssignDefaultValue(Variable destVar, IType type, ILInstruction inst) : base(inst)
        {
            this.destVar = destVar;
            this.type = type;
        }

        public override void Perform(SymexState state)
        {
            Expr value = Helpers.MakeDefaultValue(type);
            state.MemoryWrite(destVar.address, value);
        }
    }
}
