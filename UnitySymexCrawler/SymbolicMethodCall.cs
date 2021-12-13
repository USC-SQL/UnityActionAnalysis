using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class SymbolicMethodCall
    {
        public readonly IMethod method;
        public readonly List<Expr> args;
        public readonly List<BoolExpr> pathCondition;

        public SymbolicMethodCall(IMethod method, List<Expr> args, List<BoolExpr> pathCondition)
        {
            this.method = method;
            this.args = args;
            this.pathCondition = pathCondition;
        }
    }
}
