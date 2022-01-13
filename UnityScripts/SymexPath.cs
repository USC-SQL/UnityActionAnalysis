using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Z3;

public class SymexPath
{
    public readonly int pathId;
    private string condition;
    private SymexDatabase db;

    public SymexPath(int pathId, string condition, SymexDatabase db)
    {
        this.pathId = pathId;
        this.condition = condition;
        this.db = db;
    }

    private static void FindFreeVariablesSearch(Expr expr, HashSet<Symbol> seen, List<FuncDecl> result)
    {
        if (expr.IsConst && expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED)
        {
            if (seen.Add(expr.FuncDecl.Name))
            {
                result.Add(expr.FuncDecl);
            }
        } else
        {
            for (uint i = 0, n = expr.NumArgs; i < n; ++i)
            {
                FindFreeVariablesSearch(expr.Arg(i), seen, result);
            }
        }
    }

    private static List<FuncDecl> FindFreeVariables(IEnumerable<Expr> exprs)
    {
        List<FuncDecl> result = new List<FuncDecl>();
        HashSet<Symbol> seen = new HashSet<Symbol>();
        foreach (Expr expr in exprs)
        {
            FindFreeVariablesSearch(expr, seen, result);
        }
        return result;
    }

    private bool IsInputVariable(FuncDecl decl)
    {
        string name = decl.Name.ToString();
        if (name.StartsWith("symcall:"))
        {
            int symcallId = int.Parse(name.Substring(8));
            SymbolicMethodCall smc = db.GetSymbolicMethodCall(symcallId, this);
            if (smc.method.DeclaringType.FullName == "UnityEngine.Input")
            {
                if (smc.method.Name == "GetKeyDown" || smc.method.Name == "GetAxis")
                {
                    return true;
                }
            }
        }
        return false;
    }

    private class ResolutionException : Exception
    {
        public ResolutionException(string reason) : base(reason)
        {
        }
    }

    private object ResolveValue(SymexValue value, MonoBehaviour instance)
    {
        switch (value.GetValueType())
        {
            case SymexValueType.StringConstant:
                return ((SymexStringConstantValue)value).value;
            case SymexValueType.BitVecConstant:
                return ((SymexBitVecConstantValue)value).value;
            case SymexValueType.Object:
                /*{
                    var dict = ((SymexObjectValue)value).value;
                    string address = ((SymexStringConstantValue)dict["_address"]).value;
                    bool staticField;
                    object baseObj;
                    if ((staticField = address.StartsWith("staticfield:")) || address.StartsWith("frame:0:this:instancefield:"))
                    {
                        if (staticField)
                        {

                        } else
                        {

                        }
                    } else if (address.StartsWith("symcall:"))
                    {

                    } else
                    {
                        throw new Exception("cannot resolve object address " + address);
                    }
                }*/
                throw new ResolutionException("object value type unsupported");
            case SymexValueType.Z3Expr:
                throw new ResolutionException("z3 expression value type unsupported");
            default:
                throw new Exception("unrecognized value type " + value.GetValueType());
        }
    }

    private object ResolveVariable(FuncDecl decl, MonoBehaviour instance, Context z3)
    {
        string name = decl.Name.ToString();
        bool staticField;
        if ((staticField = name.StartsWith("staticfield:")) || name.StartsWith("frame:0:this:instancefield:"))
        {
            object value;
            string[] parts = name.Split(':');
            int idx;
            if (staticField)
            {
                string field = parts[1];
                int dotIndex = field.LastIndexOf('.');
                string typeName = field.Substring(0, dotIndex);
                string fieldName = field.Substring(dotIndex + 1);
                Type type = Type.GetType(typeName);
                FieldInfo f = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                value = f.GetValue(null);
                idx = 2;
            } else
            {
                string fieldName = parts[4];
                Type type = instance.GetType();
                FieldInfo f = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                value = f.GetValue(instance);
                idx = 5;
            }
            while (idx < parts.Length)
            {
                if (parts[idx] == "instancefield")
                {
                    string fieldName = parts[idx + 1];
                    Type type = value.GetType();
                    FieldInfo f = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    value = f.GetValue(instance);
                    idx += 2;
                } else
                {
                    throw new ResolutionException("expected instancefield, got " + parts[idx]);
                }
            }
            return value;
        }
        else if (name.StartsWith("symcall:"))
        {
            int symcallId = int.Parse(name.Substring(8));
            var smc = db.GetSymbolicMethodCall(symcallId, this);
            var methodParams = smc.method.GetParameters();
            object[] resolvedArgs = new object[methodParams.Length];
            for (int i = 0; i < methodParams.Length; ++i)
            {
                var arg = db.GetSymbolicMethodCallArgument(i, smc, z3);
                resolvedArgs[i] = ResolveValue(arg.value, instance);
            }
            if (smc.method.IsStatic)
            {
                return smc.method.Invoke(null, resolvedArgs);
            } else
            {
                object[] args2 = new object[resolvedArgs.Length - 1];
                for (int i = 1, n = resolvedArgs.Length; i < n; ++i)
                {
                    args2[i - 1] = resolvedArgs[i];
                }
                return smc.method.Invoke(resolvedArgs[0], args2);
            }
        } else
        {
            throw new ResolutionException("cannot resolve variable " + name);
        }
    }

    private InputCondition ModelInputVariableToCondition(Model m, FuncDecl varDecl, Expr value, Context z3)
    {
        string name = varDecl.Name.ToString();
        if (name.StartsWith("symcall:"))
        {
            int symcallId = int.Parse(name.Substring(8));
            SymbolicMethodCall smc = db.GetSymbolicMethodCall(symcallId, this);
            if (smc.method.DeclaringType.FullName == "UnityEngine.Input")
            {
                if (smc.method.Name == "GetKeyDown")
                {
                    var arg = db.GetSymbolicMethodCallArgument(0, smc, z3);
                    if (arg.value is SymexBitVecConstantValue)
                    {
                        KeyCode keyCode = (KeyCode)Enum.ToObject(typeof(KeyCode), ((SymexBitVecConstantValue)arg.value).value);
                        uint intVal = uint.Parse(value.ToString());
                        return new KeyDownInputCondition(keyCode, intVal != 0);
                    } else
                    {
                        throw new ResolutionException("cannot identify key code from " + arg.value);
                    }
                } else if (smc.method.Name == "GetAxis")
                {
                    var arg = db.GetSymbolicMethodCallArgument(0, smc, z3);
                    if (arg.value is SymexStringConstantValue)
                    {
                        string axisName = ((SymexStringConstantValue)arg.value).value;
                        var zero = z3.MkFPZero((FPSort)value.Sort, false);
                        var one = z3.MkFP(1.0, (FPSort)value.Sort);
                        var negOne = z3.MkFP(-1.0, (FPSort)value.Sort);
                        float axisValue = (float)m.Double(z3.MkITE(z3.MkFPGt((FPExpr)value, zero), one, z3.MkITE(z3.MkFPLt((FPExpr)value, zero), negOne, zero)));
                        return new AxisInputCondition(axisName, axisValue);
                    } else
                    {
                        throw new ResolutionException("cannot identify axis name from " + arg.value);
                    }
                }
            } else
            {
                throw new ResolutionException("unrecognized input symcall to method " + smc.method.Name + " in " + smc.method.DeclaringType.FullName);
            }
        }
        throw new ResolutionException("unrecognized input variable '" + name + "'");
    }

    private void ModelToInputConditions(Model m, Context z3, out ISet<InputCondition> inputConditions)
    {
        inputConditions = new HashSet<InputCondition>();
        foreach (var p in m.Consts)
        {
            var decl = p.Key;
            var value = p.Value;
            if (IsInputVariable(decl))
            {
                InputCondition cond = ModelInputVariableToCondition(m, decl, value, z3);
                inputConditions.Add(cond);
            }
        }
    }
    
    public bool CheckSatisfiable(MonoBehaviour instance, Context z3, out ISet<InputCondition> pathCondition)
    {
        using (Solver solver = z3.MkSolver())
        {
            solver.Assert(z3.ParseSMTLIB2String(condition));
            List<FuncDecl> variables = FindFreeVariables(solver.Assertions);
            foreach (FuncDecl variable in variables)
            {
                if (!IsInputVariable(variable))
                {
                    try
                    {
                        object value = ResolveVariable(variable, instance, z3);
                        var assertion = z3.MkEq(z3.MkConst(variable.Name, variable.Range), SymexHelpers.ToZ3Expr(value, variable.Range, z3));
                        solver.Assert(assertion);
                    } catch (ResolutionException e)
                    {
                        //Debug.LogWarning("Failed to resolve non-input variable " + variable + ": " + e.Message);
                    }
                }
            }
            if (solver.Check() == Status.SATISFIABLE)
            {
                try
                {
                    ModelToInputConditions(solver.Model, z3, out pathCondition);
                    return true;
                } catch (ResolutionException e)
                {
                    pathCondition = null;
                    Debug.LogWarning("Failed to resolve input variables (ignoring path): " + e.Message);
                    return false;
                }
            } else
            {
                pathCondition = null;
                return false;
            }
        }
    }
}