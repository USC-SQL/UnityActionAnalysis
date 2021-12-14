using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler
{
    public abstract class Operation
    {
        public ILInstruction Instruction { get; private set; }

        public Operation(ILInstruction instruction)
        {
            Instruction = instruction;
        }

        public abstract void Perform(SymexState state);
    }


}
