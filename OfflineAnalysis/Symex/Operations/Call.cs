using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class Call : Operation
    {
        private InstructionPointer IP;

        public Call(InstructionPointer IP, ILInstruction inst) : base(inst) 
        {
            this.IP = IP;
        }

        public override void Perform(SymexState state)
        {
            state.NewFrame();
            Fetch fetchOp = new Fetch(IP, Instruction);
            fetchOp.Perform(state);
        }
    }
}
