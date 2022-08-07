//#define LOG_RESOLUTION_WARNINGS

using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Z3;

namespace UnityActionAnalysis
{
    public class SymexPath
    {
        public readonly int pathId;
        public readonly int pathIndex;
        private BoolExpr[] condition;
        public readonly Dictionary<int, Symcall> symcalls;
        private Dictionary<FuncDecl, Func<ExprContext, object>> nonInputVars;
        public readonly Dictionary<int, List<Func<ExprContext, object>>> inputArgs;
        private Context z3;
        
        public SymexMethod Method { get; private set; }

        public SymexPath(int pathId, int pathIndex, BoolExpr[] condition, Dictionary<int, Symcall> symcalls, SymexMethod m, Context z3)
        {
            this.pathId = pathId;
            this.pathIndex = pathIndex;
            this.condition = condition;
            this.symcalls = symcalls;

            Method = m;

            inputArgs = new Dictionary<int, List<Func<ExprContext, object>>>();

            nonInputVars = new Dictionary<FuncDecl, Func<ExprContext, object>>();
            var freeVars = SymexHelpers.FindFreeVariables(condition);
            foreach (FuncDecl variable in freeVars)
            {
                int symcallId;
                if (IsInputVariable(variable, out symcallId))
                {
                    List<Func<ExprContext, object>> args = new List<Func<ExprContext, object>>();
                    Symcall sc = symcalls[symcallId];
                    foreach (SymexValue arg in sc.args)
                    {
                        Func<ExprContext, object> compiled = null;
                        try
                        {
                            compiled = ExprCompile.ResolveValue(arg, this);
                        }
                        catch (ResolutionException e)
                        {
#if LOG_RESOLUTION_WARNINGS
                            Debug.LogWarning("failed to resolve value '" + arg + "' due to: " + e.Message);
#endif
                        }
                        args.Add(compiled);
                    }
                    inputArgs.Add(symcallId, args);
                } else
                {
                    try
                    {
                        var fn = ExprCompile.ResolveVariable(variable.Name.ToString(), this);
                        nonInputVars.Add(variable, fn);
                    }
                    catch (ResolutionException e)
                    {
#if LOG_RESOLUTION_WARNINGS
                        Debug.LogWarning("failed to resolve variable '" + variable + "' due to: " + e.Message);
#endif
                    }
                }
            }

            this.z3 = z3;
        }

        public bool IsInputVariable(FuncDecl variable, out int symcallId)
        {
            string name = variable.Name.ToString();
            if (name.StartsWith("symcall:"))
            {
                symcallId = int.Parse(name.Substring(8));
                Symcall sc = symcalls[symcallId];
                if (sc.method.DeclaringType.FullName == "UnityEngine.Input")
                {
                    return true;
                }
            }
            symcallId = -1;
            return false;
        }

        public bool ContainsInputVariable(Expr e)
        {
            if (e.IsConst && e.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UNINTERPRETED && IsInputVariable(e.FuncDecl, out _))
            {
                return true;
            } else
            {
                for (uint i = 0, n = e.NumArgs; i < n; ++i)
                {
                    if (ContainsInputVariable(e.Arg(i)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckFeasible(MonoBehaviour instance, PreconditionFuncs pfuncs)
        {
            return pfuncs.preconditionFuncs[Method.method][pathIndex - 1](instance);
        }

        private InputCondition ModelInputVariableToCondition(Model m, FuncDecl varDecl, Expr value, ExprContext evalContext, Context z3)
        {
            string name = varDecl.Name.ToString();
            if (name.StartsWith("symcall:"))
            {
                int symcallId = int.Parse(name.Substring(8));
                Symcall sc = symcalls[symcallId];
                if (sc.method.DeclaringType.FullName == "UnityEngine.Input")
                {
                    if (sc.method.Name == "GetKey" || sc.method.Name == "GetKeyDown" || sc.method.Name == "GetKeyUp")
                    {
                        var arg = inputArgs[symcallId][0];
                        if (arg == null)
                        {
                            throw new ResolutionException("key code unavailable");
                        }

                        KeyCode keyCode;
                        object obj = arg(evalContext);
                        if (obj is string)
                        {
                            keyCode = InputManagerSettings.KeyNameToCode((string)obj).Value;
                        } else
                        {
                            int keyCodeVal = (int)Convert.ChangeType(obj, typeof(int));
                            keyCode = (KeyCode)Enum.ToObject(typeof(KeyCode), keyCodeVal);
                        }
                        
                        uint intVal = uint.Parse(value.ToString());
                        switch (sc.method.Name)
                        {
                            case "GetKey":
                                return new KeyInputCondition(keyCode, intVal != 0);
                            case "GetKeyDown":
                                return new KeyDownInputCondition(keyCode, intVal != 0);
                            case "GetKeyUp":
                                return new KeyUpInputCondition(keyCode, intVal != 0);
                        }
                    }
                    else if (sc.method.Name == "GetAxis" || sc.method.Name == "GetAxisRaw")
                    {
                        var arg = inputArgs[symcallId][0];
                        if (arg == null)
                        {
                            throw new ResolutionException("axis name unavailable");
                        }
                        var result = arg(evalContext);
                        if (!(result is string))
                        {
                            throw new ResolutionException("unexpected result from evaluating axis argument: " + result);
                        }
                        string axisName = (string)result;
                        var zero = z3.MkFPZero((FPSort)value.Sort, false);
                        var one = z3.MkFP(1.0, (FPSort)value.Sort);
                        var negOne = z3.MkFP(-1.0, (FPSort)value.Sort);
                        float axisValue = (float)m.Double(z3.MkITE(z3.MkFPGt((FPExpr)value, zero), one, z3.MkITE(z3.MkFPLt((FPExpr)value, zero), negOne, zero)));
                        return new AxisInputCondition(axisName, axisValue);
                    } else if (sc.method.Name == "GetButton" || sc.method.Name == "GetButtonDown" || sc.method.Name == "GetButtonUp")
                    {
                        var arg = inputArgs[symcallId][0];
                        if (arg == null)
                        {
                            throw new ResolutionException("button name unavailable");
                        }
                        var result = arg(evalContext);
                        if (!(result is string))
                        {
                            throw new ResolutionException("unexpected result from evaluating button argument: " + result);
                        }
                        string buttonName = (string)result;
                        uint intVal = uint.Parse(value.ToString());
                        switch (sc.method.Name)
                        {
                            case "GetButton":
                                return new ButtonInputCondition(buttonName, intVal > 0);
                            case "GetButtonDown":
                                return new ButtonDownInputCondition(buttonName, intVal > 0);
                            case "GetButtonUp":
                                return new ButtonUpInputCondition(buttonName, intVal > 0);
                        }
                    } else
                    {
                        throw new ResolutionException("unsupported input API " + sc.method.DeclaringType.FullName + "." + sc.method.Name);
                    }
                }
                else
                {
                    throw new ResolutionException("unrecognized input symcall to method " + sc.method.Name + " in " + sc.method.DeclaringType.FullName);
                }
            }
            throw new ResolutionException("unrecognized input variable '" + name + "'");
        }

        private void ModelToInputConditions(Model m, ExprContext evalContext, Context z3, out InputConditionSet inputConditions)
        {
            inputConditions = new InputConditionSet();
            foreach (var p in m.Consts)
            {
                var decl = p.Key;
                var value = p.Value;
                if (IsInputVariable(decl, out _))
                {
                    InputCondition cond = ModelInputVariableToCondition(m, decl, value, evalContext, z3);
                    inputConditions.Add(cond);
                }
            }
        }

        public bool SolveForInputs(MonoBehaviour instance, out InputConditionSet result)
        {
            ExprContext ctx = new ExprContext(instance);
            using var solver = z3.MkSolver();
            solver.Assert(condition);
            foreach (var kv in nonInputVars)
            {
                FuncDecl v = kv.Key;
                Func<ExprContext, object> fn = kv.Value;
                try
                {
                    object value = fn(ctx);
                    var assertion = z3.MkEq(z3.MkConst(v.Name, v.Range), SymexHelpers.ToZ3Expr(value, v.Range, z3));
                    solver.Assert(assertion);
                } catch (ResolutionException e)
                {
#if LOG_RESOLUTION_WARNINGS
                    Debug.LogWarning("failed to evaluate variable " + v.Name.ToString() + " due to: " + e.Message);
#endif
                    result = null;
                    return false;
                }
            }
            if (solver.Check() == Status.SATISFIABLE)
            {
                try
                {
                    ModelToInputConditions(solver.Model, ctx, z3, out result);
                    return true;
                } catch (ResolutionException e)
                {
#if LOG_RESOLUTION_WARNINGS
                    Debug.LogWarning("failed to resolve input variables (action will have no effect): " + e.Message);
#endif
                    result = null;
                    return false;
                }
            } else
            {
                result = null;
                return false;
            }
        }
    }
}