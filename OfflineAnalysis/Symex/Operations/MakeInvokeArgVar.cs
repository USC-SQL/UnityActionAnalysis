using System;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeInvokeArgVar : Operation
    {
        private Variable variable;
        private int argIndex;

        public MakeInvokeArgVar(Variable variable, int argIndex, ILInstruction inst) : base(inst)
        {
            this.variable = variable;
            this.argIndex = argIndex;
        }

        public override void Perform(SymexState state)
        {
            variable.address = new MemoryAddress(false, "frame:" + state.NextFrameID() + ":arg:" + argIndex);
        }
    }
}
