using System;
using Microsoft.Z3;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class DebugPrintVariable : Operation
    {
        private string tag;
        private Variable v;

        public DebugPrintVariable(string tag, Variable v, ILInstruction inst) : base(inst)
        {
            this.tag = tag;
            this.v = v;
        }

        public override void Perform(SymexState state)
        {
            Expr value = state.MemoryRead(v.address, v.type);
            Console.WriteLine("[" + tag + "] " + v.address + ": " + value);
            if (value.Sort is IntSort)
            {
                try
                {
                    Reference r = Reference.FromExpr(value);
                    Console.WriteLine("\treference:" + r.address + " of type " + r.type);
                }
                catch (ArgumentException) { }
            }
        }
    }
}
