using System;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeInvokeReturnVar : Operation
    {
        private Variable variable;

        public MakeInvokeReturnVar(Variable variable, ILInstruction inst) : base(inst)
        {
            this.variable = variable;
        }

        public override void Perform(SymexState state)
        {
            variable.address = new MemoryAddress(false, "frame:" + state.NextFrameID() + ":ret");
        }
    }
}
