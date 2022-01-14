using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public abstract class Configuration
    {
        public abstract bool IsMethodSymbolic(IMethod method);

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