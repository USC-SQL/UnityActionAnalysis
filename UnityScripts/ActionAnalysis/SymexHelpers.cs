using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Z3;

namespace UnityActionAnalysis
{
    public static class SymexHelpers
    {
        public static MethodInfo GetMethodFromSignature(string signature)
        {
            int paren = signature.IndexOf("(");
            string name = signature.Substring(0, paren);
            string[] paramTypeNames = signature.Substring(paren + 1, signature.IndexOf(")") - paren - 1).Split(';');
            if (paramTypeNames.Length == 1 && paramTypeNames[0].Length == 0)
            {
                paramTypeNames = new string[0];
            }
            string[] parts = name.Split(':');
            string declaringTypeName = parts[0];
            string methodName = parts[1];
            Type[] paramTypes = new Type[paramTypeNames.Length];
            for (int i = 0, n = paramTypeNames.Length; i < n; ++i)
            {
                Type paramType = Type.GetType(paramTypeNames[i]);
                paramTypes[i] = paramType;
            }
            Type declaringType = Type.GetType(declaringTypeName);
            return declaringType.GetMethod(methodName, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, paramTypes, null);
        }

        private static bool IsSigned(Type type)
        {
            if (type.IsEnum)
            {
                return true;
            }
            else switch (type.FullName)
                {
                    case "System.Boolean":
                    case "System.Byte":
                    case "System.Char":
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UIntPtr":
                    case "System.UInt64":
                        return false;
                    case "System.SByte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.IntPtr":
                    case "System.Int64":
                        return true;
                    default:
                        throw new ArgumentException("unexpected type " + type);
                }
        }

        public static Expr ToZ3Expr(object value, Sort sort, Context z3)
        {
            if (sort is BitVecSort)
            {
                Type type = value.GetType();
                object numericValue;
                if (value is bool)
                {
                    numericValue = ((bool)value) ? 1 : 0;
                }
                else if (type.IsEnum)
                {
                    numericValue = Convert.ChangeType(value, type.GetEnumUnderlyingType());
                }
                else
                {
                    numericValue = value;
                }
                if (IsSigned(type))
                {
                    long lval = (long)Convert.ChangeType(numericValue, typeof(long));
                    return z3.MkBV(lval, ((BitVecSort)sort).Size);
                }
                else
                {
                    ulong ulval = (ulong)Convert.ChangeType(numericValue, typeof(ulong));
                    return z3.MkBV(ulval, ((BitVecSort)sort).Size);
                }
            }
            else if (sort is FPSort)
            {
                if (value is float)
                {
                    float fval = (float)value;
                    return z3.MkFP(fval, (FPSort)sort);
                }
                else
                {
                    double dval = (double)value;
                    return z3.MkFP(dval, (FPSort)sort);
                }
            }
            else if (sort is DatatypeSort)
            {
                DatatypeSort dsort = (DatatypeSort)sort;
                Type type = value.GetType();
                var ctor = dsort.Constructors[0];
                var accessors = dsort.Accessors[0];
                Expr[] args = new Expr[accessors.Length];
                for (int i = 0; i < accessors.Length; ++i)
                {
                    var accessor = accessors[i];
                    string fieldName = accessor.Name.ToString();
                    FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    object fieldValue = field.GetValue(value);
                    args[i] = ToZ3Expr(fieldValue, accessor.Range, z3);
                }
                return ctor.Apply(args);
            }
            else
            {
                throw new ArgumentException("unrecognized sort " + sort);
            }
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

        public static List<FuncDecl> FindFreeVariables(IEnumerable<Expr> exprs)
        {
            List<FuncDecl> result = new List<FuncDecl>();
            HashSet<Symbol> seen = new HashSet<Symbol>();
            foreach (Expr expr in exprs)
            {
                FindFreeVariablesSearch(expr, seen, result);
            }
            return result;
        }
    }
}