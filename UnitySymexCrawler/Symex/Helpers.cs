using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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

        public static void DebugLog(string message)
        {
            using (StreamWriter sw = File.AppendText(@"C:\Users\sasha-usc\Misc\debug.log"))
            {
                sw.WriteLine(message);
            }
        }
    }
}
