using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public static class CodeGenHelpers
    {
        public static IType FindType(CSharpDecompiler csd, string typeName)
        {
            return csd.TypeSystem.FindType(new FullTypeName(typeName.Substring(0, typeName.LastIndexOf(","))));
        }

        public static bool IsInputVariable(FuncDecl variable, SymexState s, out int symcallId)
        {
            string name = variable.Name.ToString();
            if (name.StartsWith("symcall:"))
            {
                symcallId = int.Parse(name.Substring(8));
                var smc = s.symbolicMethodCalls[symcallId];
                if (smc.method.DeclaringType.FullName == "UnityEngine.Input")
                {
                    return true;
                }
            }
            symcallId = -1;
            return false;
        }

        public static bool ContainsInputVariable(Expr e, SymexState s)
        {
            if (e.IsConst && e.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED && IsInputVariable(e.FuncDecl, s, out _))
            {
                return true;
            }
            else
            {
                for (uint i = 0, n = e.NumArgs; i < n; ++i)
                {
                    if (ContainsInputVariable(e.Arg(i), s))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
