using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class AssignSymbolicInstanceMethodResult : Operation
    {
        private Variable resultVar;
        private IMethod method;
        private Variable thisVar;
        private List<Variable> argVars;
        
        public AssignSymbolicInstanceMethodResult(Variable resultVar, IMethod method, Variable thisVar, List<Variable> argVars, ILInstruction inst) : base(inst)
        {
            this.resultVar = resultVar;
            this.method = method;
            this.thisVar = thisVar;
            this.argVars = argVars;
        }

        public override void Perform(SymexState state)
        {
            int symId = state.symbolicMethodCounter++;
            string name = "symcall:" + symId;
            Expr value = state.MakeSymbolicValue(method.ReturnType, name);
            List<Expr> argValues = new List<Expr>(argVars.Count + 1);
            argValues.Add(state.MemoryRead(thisVar.address, thisVar.type));
            foreach (Variable argVar in argVars)
            {
                argValues.Add(state.MemoryRead(argVar.address, argVar.type));
            }
            state.symbolicMethodCalls[symId] = new SymbolicMethodCall(method, argValues, new List<BoolExpr>(state.pathCondition));
            state.MemoryWrite(resultVar.address, value);
        }
    }
}
