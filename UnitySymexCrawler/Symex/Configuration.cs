using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;
using UnitySymexCrawler.Operations;

namespace UnitySymexCrawler
{
    public abstract class Configuration
    {
        public abstract bool IsMethodSymbolic(IMethod method);

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

        public virtual int SymbolicMethodResultVarId(IMethod method, List<Expr> arguments, SymexState state)
        {
            return state.symbolicMethodCounter++;
        }
    }
}