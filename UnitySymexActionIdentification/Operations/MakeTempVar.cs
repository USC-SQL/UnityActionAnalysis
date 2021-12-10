﻿using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class MakeTempVar : Operation
    {
        private Variable variable;

        public MakeTempVar(Variable variable, ILInstruction inst) : base(inst)
        {
            this.variable = variable;
        }

        public override void Perform(SymexState state)
        {
            int tmpId = state.tempVarCounter++;
            variable.address = new MemoryAddress("F" + state.frameID + "_tmp" + tmpId);
        }
    }
}