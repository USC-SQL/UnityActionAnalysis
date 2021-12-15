using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public abstract class Configuration
    {
        public abstract bool IsMethodSymbolic(IMethod method);

        public virtual int SymbolicMethodResultVarId(IMethod method, List<Expr> arguments, SymexState state)
        {
            return state.symbolicMethodCounter++;
        }
    }
}