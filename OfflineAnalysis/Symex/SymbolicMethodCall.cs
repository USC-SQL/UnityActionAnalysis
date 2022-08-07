using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnityActionAnalysis
{
    public class SymbolicMethodCall
    {
        public readonly IMethod method;
        public readonly List<Expr> args;

        public SymbolicMethodCall(IMethod method, List<Expr> args)
        {
            this.method = method;
            this.args = args;
        }
    }
}
