using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;
using UnityActionAnalysis.Operations;

namespace UnityActionAnalysis
{
    public abstract class Configuration
    {
        public abstract bool IsMethodSummarized(IMethod method);

        public virtual void ApplyMethodSummary(IMethod method, List<Expr> arguments, Variable resultVar, SymexState state)
        {
            int symId = state.symcallCounter++;
            ApplySymcallMethodSummary(symId, method, arguments, resultVar, state);
        }

        protected Expr ApplySymcallMethodSummary(int symId, IMethod method, List<Expr> arguments, Variable resultVar, SymexState state)
        {
            string name = "symcall:" + symId;
            Expr value = state.MakeSymbolicValue(method.IsConstructor ? method.DeclaringType : method.ReturnType, name);
            state.symbolicMethodCalls[symId] = new SymbolicMethodCall(method, arguments);
            state.MemoryWrite(resultVar.address, value);
            return value;
        }

        public virtual bool ShouldSkipBranchCase(BranchCase branchCase, ILInstruction branchInst, SymexState state)
        {
            return false;
        }

        public virtual object NewStateCustomData()
        {
            return null;
        }

        public virtual object CloneStateCustomData(object data)
        {
            return null;
        }
    }
}