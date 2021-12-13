using System.Collections.Generic;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexActionIdentification.Operations
{
    public class AssignSymbolicStaticMethodResult : Operation
    {
        private Variable resultVar;
        private IMethod method;
        private List<Variable> argVars;

        public AssignSymbolicStaticMethodResult(Variable resultVar, IMethod method, List<Variable> argVars, ILInstruction inst) : base(inst)
        {
            this.resultVar = resultVar;
            this.method = method;
            this.argVars = argVars;
        }

        public override void Perform(SymexState state)
        {
            int symId = state.symbolicMethodCounter++;
            string name = "symcall:" + symId;
            Expr value = state.MakeSymbolicValue(method.ReturnType, name);
            List<Expr> argValues = new List<Expr>(argVars.Count);
            foreach (Variable argVar in argVars)
            {
                Expr argValue = state.MemoryRead(argVar.address, argVar.type);
                argValues.Add(argValue);
            }
            state.symbolicMethodCalls[symId] = new SymbolicMethodCall(method, argValues, new List<BoolExpr>(state.pathCondition));
            state.MemoryWrite(resultVar.address, value);
        }
    }
}
