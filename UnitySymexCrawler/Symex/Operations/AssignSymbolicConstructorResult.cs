using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler.Operations
{
    public class AssignSymbolicConstructorResult : Operation
    {
        private Variable resultVar;
        private IMethod ctor;
        private List<Variable> argVars;

        public AssignSymbolicConstructorResult(Variable resultVar, IMethod ctor, List<Variable> argVars, ILInstruction inst) : base(inst)
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
            int symId = SymexMachine.Instance.Config.SymbolicMethodResultVarId(ctor, argValues, state);
            string name = "symcall:" + symId;
            Expr value = state.MakeSymbolicValue(ctor.DeclaringType, name);
            state.symbolicMethodCalls[symId] = new SymbolicMethodCall(ctor, argValues);
            state.MemoryWrite(resultVar.address, value);
        }
    }
}
