﻿using System;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class MakeInvokeThisVar : Operation
    {
        private Variable variable;

        public MakeInvokeThisVar(Variable variable, ILInstruction inst) : base(inst)
        {
            this.variable = variable;
        }

        public override void Perform(SymexState state)
        {
            variable.address = new MemoryAddress("F" + state.NextFrameID() + "_this");
        }
    }
}
