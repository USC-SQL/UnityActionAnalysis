using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class Helpers
    {
        public static IEnumerable<IField> GetInstanceFields(IType type)
        {
            return type.GetFields(f => !f.IsStatic);
        }

        public static FuncDecl FindFieldAccessor(DatatypeSort dsort, IField field)
        {
            var accessors = dsort.Accessors[0];
            FuncDecl fieldAccessor = null;
            foreach (FuncDecl acc in accessors)
            {
                if (((StringSymbol)acc.Name).String == field.Name)
                {
                    fieldAccessor = acc;
                    break;
                }
            }
            if (fieldAccessor == null)
            {
                throw new Exception("failed to find accessor for field " + field.FullName);
            }
            return fieldAccessor;
        }

        public static ILFunction GetInstructionFunction(ILInstruction inst)
        {
            while (!(inst is ILFunction))
            {
                inst = inst.Parent;
            }
            return (ILFunction)inst;
        }

        public static Expr MakeDefaultValue(IType type)
        {
            Context z3 = SymexMachine.Instance.Z3;
            if (type.Kind == TypeKind.Class || type.Kind == TypeKind.Interface || type.Kind == TypeKind.Array)
            {
                Reference r = new Reference(type);
                return r.ToExpr();
            }
            else if (type.Kind == TypeKind.Struct || type.Kind == TypeKind.Enum)
            {
                var sort = SymexMachine.Instance.SortPool.TypeToSort(type);
                if (sort is DatatypeSort) // struct with fields
                {
                    var dsort = (DatatypeSort)sort;
                    List<Expr> elems = new List<Expr>();
                    foreach (IField field in Helpers.GetInstanceFields(type))
                    {
                        elems.Add(MakeDefaultValue(field.Type));
                    }
                    FuncDecl ctor = dsort.Constructors[0];
                    return ctor.Apply(elems.ToArray());
                }
                else if (sort is BitVecSort)
                {
                    BitVecSort bvsort = (BitVecSort)sort;
                    return z3.MkBV(0, bvsort.Size);
                }
                else if (sort is FPSort)
                {
                    FPSort fpsort = (FPSort)sort;
                    return z3.MkFP(0.0, fpsort);
                }
                else
                {
                    throw new Exception("unexpected sort " + sort);
                }
            }
            else
            {
                throw new NotImplementedException("MakeDefaultValue for type of kind " + type.Kind + " not implemented");
            }
        }

        private static IEnumerable<FPExpr> FindFloatConstsVisitor(Expr expr)
        {
            if (expr is FPExpr && expr.IsConst && expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED)
            {
                yield return (FPExpr)expr;
            } else
            {
                for (uint i = 0, n = expr.NumArgs; i < n; ++i)
                {
                    foreach (FPExpr c in FindFloatConstsVisitor(expr.Arg(i)))
                    {
                        yield return c;
                    }
                }
            }
        }

        public static IEnumerable<FPExpr> FindFloatConsts(Expr expr)
        {
            foreach (FPExpr c in FindFloatConstsVisitor(expr))
            {
                yield return c;
            }
        }

        public static void AssertAssumptions(Solver s, Context z3)
        {
            // assume no NaN
            Dictionary<int, FPExpr> floatConsts = new Dictionary<int, FPExpr>();
            foreach (BoolExpr cond in s.Assertions)
            {
                foreach (FPExpr c in FindFloatConsts(cond))
                {
                    floatConsts[c.FuncDecl.Name.GetHashCode()] = c;
                }
            }
            foreach (var p in floatConsts)
            {
                s.Assert(z3.MkNot(z3.MkFPIsNaN(p.Value)));
            }
        }

        public static string GetAssemblyQualifiedName(IType type)
        {
            var paramTypeDef = type.GetDefinition();
            string suffix = "";
            if (paramTypeDef != null)
            {
                string assemblyName = paramTypeDef.ParentModule.AssemblyName;
                if (assemblyName != "System.Private.CoreLib")
                {
                    suffix = "," + assemblyName;
                }
            }
            return type.ReflectionName + suffix;
        }

        public static string GetMethodSignature(IMethod method)
        {
            return GetAssemblyQualifiedName(method.DeclaringType) + ":"
                + method.Name + "(" + string.Join(";", method.Parameters.Select(param => Helpers.GetAssemblyQualifiedName(param.Type))) + ")";
        }

        private static void FindFreeVariablesSearch(Expr expr, HashSet<Symbol> seen, List<FuncDecl> result)
        {
            if (expr.IsConst && expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED)
            {
                if (seen.Add(expr.FuncDecl.Name))
                {
                    result.Add(expr.FuncDecl);
                }
            }
            else
            {
                for (uint i = 0, n = expr.NumArgs; i < n; ++i)
                {
                    FindFreeVariablesSearch(expr.Arg(i), seen, result);
                }
            }
        }

        public static List<FuncDecl> FindFreeVariables(Expr expr)
        {
            List<FuncDecl> result = new List<FuncDecl>();
            HashSet<Symbol> seen = new HashSet<Symbol>();
            FindFreeVariablesSearch(expr, seen, result);
            return result;
        }

        public static void DebugLog(string message)
        {
            using (StreamWriter sw = File.AppendText(@"C:\Users\sasha-usc\Misc\debug.log"))
            {
                sw.WriteLine(message);
            }
        }

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

        public static ISet<BoolExpr> GetInputConditions(SymexState s)
        {
            ISet<BoolExpr> result = new HashSet<BoolExpr>();
            Dictionary<BoolExpr, List<FuncDecl>> condFreeVars = new Dictionary<BoolExpr, List<FuncDecl>>();
            foreach (BoolExpr cond in s.pathCondition)
            {
                condFreeVars.Add(cond, Helpers.FindFreeVariables(cond));
            }
            ISet<Symbol> relevantVars = new HashSet<Symbol>();
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (BoolExpr cond in s.pathCondition)
                {
                    if (!result.Contains(cond))
                    {
                        bool include = false;
                        List<FuncDecl> vars = condFreeVars[cond];
                        foreach (FuncDecl v in vars)
                        {
                            if (relevantVars.Contains(v.Name) || IsInputVariable(v, s, out _))
                            {
                                include = true;
                                break;
                            }
                        }
                        if (include)
                        {
                            result.Add(cond);
                            foreach (FuncDecl v in vars)
                            {
                                relevantVars.Add(v.Name);
                            }
                            changed = true;
                        }
                    }
                }
            }
            return result;
        }

        private static void RelevantSymcallSearch(Expr e, SymexState s, ISet<int> result)
        {
            if (e.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED)
            {
                string name = e.FuncDecl.Name.ToString();
                if (name.StartsWith("symcall:"))
                {
                    int symcallId = int.Parse(name.Substring(8));
                    result.Add(symcallId);
                    var smc = s.symbolicMethodCalls[symcallId];
                    foreach (Expr arg in smc.args)
                    {
                        RelevantSymcallSearch(arg, s, result);
                    }
                }
            }
            for (uint i = 0, n = e.NumArgs; i < n; ++i)
            {
                RelevantSymcallSearch(e.Arg(i), s, result);
            }
        }

        public static ISet<int> GetRelevantSymcalls(IEnumerable<Expr> conditions, SymexState s)
        {
            ISet<int> result = new HashSet<int>();
            foreach (Expr cond in conditions)
            {
                RelevantSymcallSearch(cond, s, result);
            }
            return result;
        }
    }
}
