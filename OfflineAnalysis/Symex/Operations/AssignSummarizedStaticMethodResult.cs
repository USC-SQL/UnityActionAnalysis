using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class AssignSummarizedStaticMethodResult : Operation
    {
        private Variable resultVar;
        private IMethod method;
        private List<Variable> argVars;

        public AssignSummarizedStaticMethodResult(Variable resultVar, IMethod method, List<Variable> argVars, ILInstruction inst) : base(inst)
        {
            this.resultVar = resultVar;
            this.method = method;
            this.argVars = argVars;
        }

        public override void Perform(SymexState state)
        {
            List<Expr> argValues = new List<Expr>(argVars.Count);
            foreach (Variable argVar in argVars)
            {
                Expr argValue = state.MemoryRead(argVar.address, argVar.type);
                argValues.Add(argValue);
            }
            SymexMachine.Instance.Config.ApplyMethodSummary(method, argValues, resultVar, state);
        }
    }
}
