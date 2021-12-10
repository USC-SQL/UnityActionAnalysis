using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
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
            int symId = state.symbolicMethodCounter++;
            string name = ctor.FullName + "_" + symId;
            Expr value = state.MakeSymbolicValue(ctor.DeclaringType, name);
            List<Expr> argValues = new List<Expr>(argVars.Count);
            foreach (Variable argVar in argVars)
            {
                argValues.Add(state.MemoryRead(argVar.address, argVar.type));
            }
            state.symbolicMethodCalls[name] = new SymbolicMethodCall(ctor, argValues);
            state.MemoryWrite(resultVar.address, value);
        }
    }
}
