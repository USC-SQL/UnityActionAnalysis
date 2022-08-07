using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class AssignSummarizedConstructorResult : Operation
    {
        private Variable resultVar;
        private IMethod ctor;
        private List<Variable> argVars;

        public AssignSummarizedConstructorResult(Variable resultVar, IMethod ctor, List<Variable> argVars, ILInstruction inst) : base(inst)
        {
            this.resultVar = resultVar;
            this.ctor = ctor;
            this.argVars = argVars;
        }

        public override void Perform(SymexState state)
        {
            List<Expr> argValues = new List<Expr>(argVars.Count);
            foreach (Variable argVar in argVars)
            {
                argValues.Add(state.MemoryRead(argVar.address, argVar.type));
            }
            SymexMachine.Instance.Config.ApplyMethodSummary(ctor, argValues, resultVar, state);
        }
    }
}
