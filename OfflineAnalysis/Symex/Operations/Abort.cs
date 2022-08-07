using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class Abort : Operation
    {
        public Abort(ILInstruction inst) : base(inst)
        {
        }

        public override void Perform(SymexState state)
        {
            state.execStatus = ExecutionStatus.ABORTED;
        }
    }
}
