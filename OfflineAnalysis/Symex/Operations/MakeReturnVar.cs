using System;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeReturnVar : Operation
    {
        private Variable variable;

        public MakeReturnVar(Variable variable, ILInstruction inst) : base(inst)
        {
            this.variable = variable;
        }

        public override void Perform(SymexState state)
        {
            variable.address = new MemoryAddress(false, "frame:" + state.frameID + ":ret");
        }
    }
}
