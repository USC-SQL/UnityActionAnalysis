using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class Return : Operation
    {
        public Return(ILInstruction inst) : base(inst)
        {
        }

        public override void Perform(SymexState state)
        {
            state.ExitFrame();
        }
    }
}
