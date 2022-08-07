using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Z3;

namespace UnityActionAnalysis
{
    public static class ExprCompile
    {
        public static Func<ExprContext, object> ResolveValue(SymexValue value, SymexPath p)
        {
            switch (value.GetValueType())
            {
                case SymexValueType.StringConstant:
                    {
                        SymexStringConstantValue sv = (SymexStringConstantValue)value;
                        string s = sv.value;
                        return ctx => s;
                    }
                case SymexValueType.BitVecConstant:
                    {
                        SymexBitVecConstantValue bv = (SymexBitVecConstantValue)value;
                        ulong v = bv.value;
                        return ctx => v;
                    }
                case SymexValueType.Object:
                    {
                        throw new ResolutionException("object resolution not yet implemented");
                    }
                case SymexValueType.Variable:
                    {
                        SymexVariableValue vv = (SymexVariableValue)value;
                        return ResolveVariable(vv.varName, p);
                    }
                case SymexValueType.Struct:
                    {
                        SymexStructValue sval = (SymexStructValue)value;
                        Type type = sval.structType;
                        var ctor = type.GetConstructor(new Type[0]);
                        Dictionary<FieldInfo, Func<ExprContext, object>> fields = new Dictionary<FieldInfo, Func<ExprContext, object>>();
                        foreach (var kv in sval.value)
                        {
                            var fieldName = kv.Key;
                            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            var fieldVal = ResolveValue(kv.Value, p);
                            fields.Add(field, fieldVal);
                        }
                        return ctx =>
                        {
                            object result = ctor.Invoke(new object[0]);
                            foreach (var kv in fields)
                            {
                                var field = kv.Key;
                                var val = kv.Value;
                                field.SetValue(result, val(ctx));
                            }
                            return result;
                        };
                    }
                case SymexValueType.UnevaluatedMethodCall:
                    {
                        SymexUnevaluatedMethodCallValue mval = (SymexUnevaluatedMethodCallValue)value;
                        List<Func<ExprContext, object>> args = new List<Func<ExprContext, object>>();
                        for (int i = 0; i < mval.arguments.Count; ++i)
                        {
                            args.Add(ResolveValue(mval.arguments[i], p));
                        }
                        if (mval.method.IsStatic)
                        {
                            return ctx =>
                            {
                                object[] argVals = new object[args.Count];
                                for (int i = 0; i < args.Count; ++i)
                                {
                                    argVals[i] = args[i](ctx);
                                }
                                return mval.method.Invoke(null, argVals);
                            };
                        } else
                        {
                            return ctx =>
                            {
                                object thisVal = args[0](ctx);
                                object[] argVals = new object[args.Count - 1];
                                for (int i = 1; i < args.Count; ++i)
                                {
                                    argVals[i - 1] = args[i](ctx);
                                }
                                return mval.method.Invoke(thisVal, argVals);
                            };
                        }
                    }
                default:
                    throw new Exception("unexpected value type " + value.GetValueType());
            }
        }

        public static Func<ExprContext, object> ResolveSymcall(Symcall sc, SymexPath p)
        {
            List<Func<ExprContext, object>> args = new List<Func<ExprContext, object>>();
            for (int i = 0, n = sc.args.Count; i < n; ++i)
            {
                args.Add(ResolveValue(sc.args[i], p));
            }
            if (sc.method.IsStatic)
            {
                return ctx =>
                {
                    object[] argVals = new object[args.Count];
                    for (int i = 0; i < args.Count; ++i)
                    {
                        argVals[i] = args[i](ctx);
                    }
                    return sc.method.Invoke(null, argVals);
                };
            }
            else
            {
                return ctx =>
                {
                    object thisVal = args[0](ctx);
                    object[] argVals = new object[args.Count - 1];
                    for (int i = 1; i < args.Count; ++i)
                    {
                        argVals[i - 1] = args[i](ctx);
                    }
                    return sc.method.Invoke(thisVal, argVals);
                };
            }
        }

        public static Func<ExprContext, object> ResolveVariable(string varName, SymexPath p)
        {
            SymexMethod m = p.Method;
            string[] parts = varName.Split(':');
            bool staticField;
            if ((staticField = varName.StartsWith("staticfield:")) || varName.StartsWith("frame:0:this:instancefield:"))
            {
                List<FieldInfo> fieldAccesses = new List<FieldInfo>();
                int idx;
                if (staticField)
                {
                    string field = parts[1];
                    int dotIndex = field.LastIndexOf('.');
                    string typeName = field.Substring(0, dotIndex);
                    string fieldName = field.Substring(dotIndex + 1);
                    Type type = Type.GetType(typeName);
                    FieldInfo f = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    fieldAccesses.Add(f);
                    idx = 2;
                }
                else
                {
                    string fieldName = parts[4];
                    Type instanceType = m.method.DeclaringType;
                    FieldInfo f = instanceType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    fieldAccesses.Add(f);
                    idx = 5;
                }
                while (idx < parts.Length)
                {
                    var lastField = fieldAccesses[fieldAccesses.Count - 1];
                    if (parts[idx] == "instancefield")
                    {
                        string fieldName = parts[idx + 1];
                        Type type = lastField.FieldType;
                        FieldInfo f = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        fieldAccesses.Add(f);
                        idx += 2;
                    } else
                    {
                        throw new Exception("expected instancefield, got " + parts[idx]);
                    }
                }
                return ctx =>
                {
                    object value = ctx.instance;
                    foreach (FieldInfo f in fieldAccesses)
                    {
                        if (value == null)
                        {
                            throw new ResolutionException("null pointer dereference when evaluating " + varName);
                        }
                        value = f.GetValue(value);
                    }
                    return value;
                };
            } else if (varName.StartsWith("symcall:"))
            {
                int symcallId = int.Parse(parts[1]);
                List<FieldInfo> fieldAccesses = new List<FieldInfo>();
                Symcall sc = p.symcalls[symcallId];
                Type currType = sc.method.ReturnType;
                var symcall = ResolveSymcall(sc, p);
                int idx = 2;
                while (idx < parts.Length)
                {
                    if (parts[idx] == "instancefield")
                    {
                        string fieldName = parts[idx + 1];
                        FieldInfo f = currType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        fieldAccesses.Add(f);
                        currType = f.FieldType;
                        idx += 2;
                    }
                    else
                    {
                        throw new Exception("expected instancefield, got " + parts[idx]);
                    }
                }
                return ctx =>
                {
                    object value = symcall(ctx);
                    foreach (FieldInfo f in fieldAccesses)
                    {
                        if (value == null)
                        {
                            throw new ResolutionException("null pointer dereference when evaluating " + varName);
                        }
                        value = f.GetValue(value);
                    }
                    return value;
                };
            } else
            {
                throw new ResolutionException("cannot resolve variable " + varName);
            }
        }

        public static Func<ExprContext, object> Compile(Expr expr, SymexPath p)
        {
            switch (expr.FuncDecl.DeclKind)
            {
                case Z3_decl_kind.Z3_OP_NOT:
                    {
                        var val = Compile(expr.Arg(0), p);
                        return ctx => !CompileHelpers.ToBool(val(ctx));
                    }
                case Z3_decl_kind.Z3_OP_AND:
                    {
                        if (expr.NumArgs > 2)
                        {
                            throw new ResolutionException("more than 2 args not supported in and expression");
                        }
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToBool(val1(ctx)) && CompileHelpers.ToBool(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_OR:
                    {
                        if (expr.NumArgs > 2)
                        {
                            throw new ResolutionException("more than 2 args not supported in or expression");
                        }
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToBool(val1(ctx)) || CompileHelpers.ToBool(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_EQ:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.Equals(val1(ctx), val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_EQ:
                    {
                        if (expr.NumArgs == 3)
                        {
                            var val1 = Compile(expr.Arg(1), p);
                            var val2 = Compile(expr.Arg(2), p);
                            return ctx => CompileHelpers.ToDouble(val1(ctx)) == CompileHelpers.ToDouble(val2(ctx));
                        } else
                        {
                            var val1 = Compile(expr.Arg(0), p);
                            var val2 = Compile(expr.Arg(1), p);
                            return ctx => CompileHelpers.ToDouble(val1(ctx)) == CompileHelpers.ToDouble(val2(ctx));
                        }
                    }
                case Z3_decl_kind.Z3_OP_ITE:
                    {
                        var cond = Compile(expr.Arg(0), p);
                        var trueVal = Compile(expr.Arg(1), p);
                        var falseVal = Compile(expr.Arg(2), p);
                        return ctx =>
                        {
                            if (CompileHelpers.ToBool(cond(ctx)))
                            {
                                return trueVal(ctx);
                            } else
                            {
                                return falseVal(ctx);
                            }
                        };
                    }
                case Z3_decl_kind.Z3_OP_FPA_GT:
                    {
                        if (expr.NumArgs == 3)
                        {
                            var val1 = Compile(expr.Arg(1), p);
                            var val2 = Compile(expr.Arg(2), p);
                            return ctx => CompileHelpers.ToDouble(val1(ctx)) > CompileHelpers.ToDouble(val2(ctx));
                        } else
                        {
                            var val1 = Compile(expr.Arg(0), p);
                            var val2 = Compile(expr.Arg(1), p);
                            return ctx => CompileHelpers.ToDouble(val1(ctx)) > CompileHelpers.ToDouble(val2(ctx));
                        }
                    }
                case Z3_decl_kind.Z3_OP_FPA_GE:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), p);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), p);
                        return ctx => CompileHelpers.ToDouble(val1(ctx)) >= CompileHelpers.ToDouble(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_LT:
                    {
                        if (expr.NumArgs == 3)
                        {
                            var val1 = Compile(expr.Arg(1), p);
                            var val2 = Compile(expr.Arg(2), p);
                            return ctx => CompileHelpers.ToDouble(val1(ctx)) < CompileHelpers.ToDouble(val2(ctx));
                        } else
                        {
                            var val1 = Compile(expr.Arg(0), p);
                            var val2 = Compile(expr.Arg(1), p);
                            return ctx => CompileHelpers.ToDouble(val1(ctx)) < CompileHelpers.ToDouble(val2(ctx));
                        }
                    }
                case Z3_decl_kind.Z3_OP_FPA_IS_NAN:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        return ctx => double.IsNaN(CompileHelpers.ToDouble(val1(ctx)));
                    }
                case Z3_decl_kind.Z3_OP_BADD:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) + CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_ADD:
                    {
                        var val1 = Compile(expr.Arg(1), p);
                        var val2 = Compile(expr.Arg(2), p);
                        return ctx => CompileHelpers.ToDouble(val1(ctx)) + CompileHelpers.ToDouble(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BSUB:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) - CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_SUB:
                    {
                        var val1 = Compile(expr.Arg(1), p);
                        var val2 = Compile(expr.Arg(2), p);
                        return ctx => CompileHelpers.ToDouble(val1(ctx)) - CompileHelpers.ToDouble(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BMUL:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) * CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_MUL:
                    {
                        var val1 = Compile(expr.Arg(1), p);
                        var val2 = Compile(expr.Arg(2), p);
                        return ctx => CompileHelpers.ToDouble(val1(ctx)) * CompileHelpers.ToDouble(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BUREM:
                case Z3_decl_kind.Z3_OP_BSREM:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) % CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_REM:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), p);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), p);
                        return ctx => CompileHelpers.ToDouble(val1(ctx)) % CompileHelpers.ToDouble(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BAND:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) & CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BOR:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) | CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BXOR:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) ^ CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BSHL:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.Shl(CompileHelpers.ToUlong(val1(ctx)), CompileHelpers.ToUlong(val2(ctx)));
                    }
                case Z3_decl_kind.Z3_OP_BASHR:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.Shr(CompileHelpers.ToUlong(val1(ctx)), CompileHelpers.ToUlong(val2(ctx)));
                    }
                case Z3_decl_kind.Z3_OP_ULT:
                case Z3_decl_kind.Z3_OP_SLT:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) < CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_LE:
                case Z3_decl_kind.Z3_OP_ULEQ:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) <= CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_UGT:
                case Z3_decl_kind.Z3_OP_SGT:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) > CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_GE:
                case Z3_decl_kind.Z3_OP_UGEQ:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) >= CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BUDIV:
                case Z3_decl_kind.Z3_OP_BSDIV:
                    {
                        var val1 = Compile(expr.Arg(0), p);
                        var val2 = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val1(ctx)) / CompileHelpers.ToUlong(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_DIV:
                    {
                        var val1 = Compile(expr.Arg(1), p);
                        var val2 = Compile(expr.Arg(2), p);
                        return ctx => CompileHelpers.ToDouble(val1(ctx)) / CompileHelpers.ToDouble(val2(ctx));
                    }
                case Z3_decl_kind.Z3_OP_CONCAT:
                    {
                        // concat only used to extend a value with 0s, so ignore the first argument
                        var val = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val(ctx));
                    }
                case Z3_decl_kind.Z3_OP_EXTRACT:
                    {
                        var val = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val(ctx));
                    }
                case Z3_decl_kind.Z3_OP_FPA_TO_FP:
                case Z3_decl_kind.Z3_OP_FPA_TO_FP_UNSIGNED:
                    {
                        return Compile(expr.Arg(1), p);
                    }
                case Z3_decl_kind.Z3_OP_FPA_TO_SBV:
                case Z3_decl_kind.Z3_OP_FPA_TO_UBV:
                    {
                        var val = Compile(expr.Arg(1), p);
                        return ctx => CompileHelpers.ToUlong(val(ctx));
                    }
                case Z3_decl_kind.Z3_OP_BNUM:
                    {
                        var val = ulong.Parse(expr.ToString());
                        return ctx => val;
                    }
                case Z3_decl_kind.Z3_OP_FPA_PLUS_INF:
                    {
                        var val = double.PositiveInfinity;
                        return ctx => val;
                    }
                case Z3_decl_kind.Z3_OP_FPA_MINUS_INF:
                    {
                        var val = double.NegativeInfinity;
                        return ctx => val;
                    }
                case Z3_decl_kind.Z3_OP_FPA_PLUS_ZERO:
                    {
                        var val = +0.0;
                        return ctx => val;
                    }
                case Z3_decl_kind.Z3_OP_FPA_MINUS_ZERO:
                    {
                        var val = -0.0;
                        return ctx => val;
                    }
                case Z3_decl_kind.Z3_OP_FPA_NUM:
                    {
                        var val = double.Parse(expr.ToString());
                        return ctx => val;
                    }
                case Z3_decl_kind.Z3_OP_TRUE:
                    return ctx => true;
                case Z3_decl_kind.Z3_OP_FALSE:
                    return ctx => false;
                case Z3_decl_kind.Z3_OP_UNINTERPRETED:
                    {
                        var val = ResolveVariable(expr.FuncDecl.Name.ToString(), p);
                        return ctx =>
                        {
                            return val(ctx);
                        };
                    }
                default:
                    throw new ResolutionException("unsupported expr: " + expr + " (kind " + expr.FuncDecl.DeclKind + ")");
            }
        }
    }
}
