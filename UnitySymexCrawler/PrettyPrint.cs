using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class PrettyPrint
    {
        private SymexMachine machine;
        private Context z3;

        public PrettyPrint(SymexMachine m, Context z3)
        {
            machine = m;
            this.z3 = z3;
        }

        private string ExprSimplifyToString(Expr e)
        {
            if (e.IsConst && e.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED)
            {
                return "|" + e.FuncDecl.Name.ToString() + "|";
            }
            string funcName = e.FuncDecl.Name.ToString();
            if (funcName == "if")
            {
                BitVecExpr bvExpr = (BitVecExpr)e.Arg(1);
                if (int.Parse(bvExpr.ToString()) == 1)
                {
                    return ExprSimplifyToString(e.Arg(0));
                } else
                {
                    return "!(" + ExprSimplifyToString(e.Arg(0)) + ")";
                }
            } else if (funcName == "bv")
            {
                return e.ToString();
            } else if (funcName == "not")
            {
                string inner = ExprSimplifyToString(e.Arg(0));
                if (inner.StartsWith("!("))
                {
                    return inner.Substring(2, inner.Length - 3);
                }
                else
                {
                    return "!(" + inner + ")";
                }
            }
            else
            {
                if (funcName == "=")
                {
                    string arg1s = e.Arg(1).ToString();
                    if (arg1s == "1")
                    {
                        return ExprSimplifyToString(e.Arg(0));
                    } else if (arg1s == "0")
                    {
                        string inner = ExprSimplifyToString(e.Arg(0));
                        if (inner.StartsWith("!("))
                        {
                            return inner.Substring(2, inner.Length - 3);
                        } else
                        {
                            return "!(" + inner + ")";
                        }
                    }
                }
                return "(" + funcName + (e.NumArgs > 0 ? " " : "") + string.Join(" ", e.Args.Select(arg => ExprSimplifyToString(arg))) + ")";
            }
        }

        private string ExprToPrettyString(Expr e, SymexState st)
        {
            if (e is IntExpr)
            {
                Reference r = Reference.FromExpr(e);
                if (r.address == null)
                {
                    return "null";
                }
                else if (r.address.heap && r.address.components.Count == 0)
                {
                    HeapObject obj = st.objects[r.address.root];
                    if (obj.value.TryGetValue("_string", out Expr strExpr))
                    {
                        var str = strExpr.ToString();
                        return str;
                    } else
                    {
                        string result = "{" + string.Join(", ", obj.value.Select(p => p.Key + ": " + ExprToPrettyString(p.Value, st))) + "}";
                        if (obj.symbolName != null)
                        {
                            return "(" + obj.symbolName + " + " + result + ")";
                        } else
                        {
                            return result;
                        }
                    }
                } else
                {
                    return ExprToPrettyString(st.MemoryRead(r.address, r.type), st);
                }
            } else
            {
                string s = ExprSimplifyToString(e);

                List<FuncDecl> vars = Helpers.FindFreeVariables(e);
                foreach (FuncDecl v in vars)
                {
                    string name = v.Name.ToString();
                    string replace = null;
                    if (name.StartsWith("symcall:"))
                    {
                        int accessorSepIndex = name.IndexOf(':', 8);
                        int symcallId;
                        string accessor;
                        if (accessorSepIndex >= 0)
                        {
                            symcallId = int.Parse(name.Substring(8, accessorSepIndex - 8));
                            accessor = name.Substring(accessorSepIndex + 1);
                        }
                        else
                        {
                            symcallId = int.Parse(name.Substring(8));
                            accessor = null;
                        }
                        var smc = st.symbolicMethodCalls[symcallId];
                        replace = smc.method.FullName + "(" + string.Join(", ", smc.args.Select(arg => ExprToPrettyString(arg, st))) + ")";
                    }

                    if (replace != null)
                    {
                        s = s.Replace("|" + name + "|", replace);
                    }
                }
                return s;
            }
        }

        public void WritePrettyPathCondition(SymexState st, StreamWriter sw)
        {
            for (int i = 0, n = st.pathCondition.Count; i < n; ++i)
            {
                Expr cond = st.pathCondition[i];
                sw.WriteLine(ExprToPrettyString(cond, st));
                if (i  < n - 1)
                {
                    sw.WriteLine("\n && \n");
                }
            }
        }

        public void WritePaths(StreamWriter sw)
        {
            foreach (SymexState s in machine.States)
            {
                WritePrettyPathCondition(s, sw);
                sw.WriteLine("\n--------------------\n");
            }
        }
    }
}
