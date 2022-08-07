using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeRef : Operation
    {
        private Variable targetVar;
        private Variable resultVar;

        public MakeRef(Variable targetVar, Variable resultVar, ILInstruction inst) : base(inst)
        {
            this.targetVar = targetVar;
            this.resultVar = resultVar;
        }

        public override void Perform(SymexState state)
        {
            Reference r = new Reference(targetVar.type, targetVar.address);
            state.MemoryRead(r.address, r.type);
            state.MemoryWrite(resultVar.address, r.ToExpr());
        }
    }
}
