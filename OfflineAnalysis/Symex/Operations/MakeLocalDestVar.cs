using System;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class MakeLocalDestVar : Operation
    {
        private Variable variable;
        private ILVariable localDestVar;

        public MakeLocalDestVar(Variable variable, ILVariable localDestVar, ILInstruction inst) : base(inst)
        {
            this.variable = variable;
            this.localDestVar = localDestVar;
        }

        public override void Perform(SymexState state)
        {
            switch (localDestVar.Kind)
            {
                case VariableKind.StackSlot:
                case VariableKind.Local:
                    variable.address = new MemoryAddress(false, "frame:" + state.frameID + ":local:" + localDestVar.Name);
                    break;
                case VariableKind.Parameter:
                    if (localDestVar.Index < 0)
                    {
                        variable.address = new MemoryAddress(false, "frame:" + state.frameID + ":this");
                    } else
                    {
                        variable.address = new MemoryAddress(false, "frame:" + state.frameID + ":arg:" + localDestVar.Index);
                    }
                    break;
                default:
                    throw new NotSupportedException("MakeLocalDestVar with ILVariable of kind " + localDestVar.Kind);
            }
        }
    }
}
