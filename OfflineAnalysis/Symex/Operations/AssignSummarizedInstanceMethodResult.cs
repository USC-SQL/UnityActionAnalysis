using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnityActionAnalysis.Operations
{
    public class AssignSummarizedInstanceMethodResult : Operation
    {
        private Variable resultVar;
        private IMethod method;
        private Variable thisVar;
        private List<Variable> argVars;
        
        public AssignSummarizedInstanceMethodResult(Variable resultVar, IMethod method, Variable thisVar, List<Variable> argVars, ILInstruction inst) : base(inst)
        {
            this.resultVar = resultVar;
            this.method = method;
            this.thisVar = thisVar;
            this.argVars = argVars;
        }

        public override void Perform(SymexState state)
        {
            List<Expr> argValues = new List<Expr>(argVars.Count + 1);
            argValues.Add(state.MemoryRead(thisVar.address, thisVar.type));
            foreach (Variable argVar in argVars)
            {
                argValues.Add(state.MemoryRead(argVar.address, argVar.type));
            }
            SymexMachine.Instance.Config.ApplyMethodSummary(method, argValues, resultVar, state);
        }
    }
}
