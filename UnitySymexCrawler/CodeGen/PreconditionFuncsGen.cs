﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class PreconditionFuncsGen
    {
        private CodeCompileUnit pfuncsUnit;
        private CodeTypeDeclaration pfuncsClass;

        private Dictionary<string, string> fieldAccessors;
        private List<CodeMemberField> fieldAccessorFields;
        private List<CodeStatement> fieldAccessorAssignStmts;

        private Dictionary<string, string> methodAccessors;
        private List<CodeMemberField> methodAccessorFields;
        private List<CodeStatement> methodAccessorAssignStmts;

        private List<CodeMemberMethod> pathMethods;
        private List<CodeStatement> pathFuncAssignStmts;

        public PreconditionFuncsGen()
        {
            pfuncsUnit = new CodeCompileUnit();
            CodeNamespace ns = new CodeNamespace("UnitySymexCrawler");
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Reflection"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            ns.Imports.Add(new CodeNamespaceImport("UnityEngine"));

            pfuncsClass = new CodeTypeDeclaration("PreconditionFuncs");
            pfuncsClass.IsClass = true;
            pfuncsClass.TypeAttributes = TypeAttributes.Public;
            ns.Types.Add(pfuncsClass);
            pfuncsUnit.Namespaces.Add(ns);

            fieldAccessors = new Dictionary<string, string>();
            fieldAccessorFields = new List<CodeMemberField>();
            fieldAccessorAssignStmts = new List<CodeStatement>();
            methodAccessors = new Dictionary<string, string>();
            methodAccessorFields = new List<CodeMemberField>();
            methodAccessorAssignStmts = new List<CodeStatement>();
            pathMethods = new List<CodeMemberMethod>();
            pathFuncAssignStmts = new List<CodeStatement>();
        }

        private CodeExpression FieldAccess(CodeExpression targetObject, IField field)
        {
            string accessor;
            if (!fieldAccessors.TryGetValue(field.FullName, out accessor))
            {
                accessor = "f_" + field.FullName.Replace(".", "_");

                CodeMemberField accessorField = new CodeMemberField();
                accessorField.Attributes = MemberAttributes.Private;
                accessorField.Name = accessor;
                accessorField.Type = new CodeTypeReference(typeof(FieldInfo));
                fieldAccessorFields.Add(accessorField);

                CodeStatement assignStmt =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), accessor),
                        new CodeMethodInvokeExpression(new CodeTypeOfExpression(field.DeclaringType.FullName), "GetField",
                            new CodePrimitiveExpression(field.Name), new CodeVariableReferenceExpression("bindingFlags")));

                fieldAccessors.Add(field.FullName, accessor);
                fieldAccessorAssignStmts.Add(assignStmt);
            }
            return new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), accessor), "GetValue", targetObject);
        }

        private CodeExpression MethodInvoke(IMethod method, CodeExpression targetObject, params CodeExpression[] args)
        {
            string accessor;
            string sig = Helpers.GetMethodSignature(method);
            if (!methodAccessors.TryGetValue(sig, out accessor))
            {
                int i = 0;
                do
                {
                    accessor = "m_" + method.FullName.Replace(".", "_") + (i > 0 ? "_" + i : "");
                    ++i;
                } while (methodAccessors.ContainsValue(accessor));

                CodeMemberField accessorField = new CodeMemberField();
                accessorField.Attributes = MemberAttributes.Private;
                accessorField.Name = accessor;
                accessorField.Type = new CodeTypeReference(typeof(MethodInfo));
                methodAccessorFields.Add(accessorField);

                CodeStatement assignStmt =
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), accessor),
                        new CodeMethodInvokeExpression(new CodeTypeOfExpression(method.DeclaringType.FullName), "GetMethod",
                            new CodePrimitiveExpression(method.Name), new CodeVariableReferenceExpression("bindingFlags")));

                methodAccessors.Add(sig, accessor);
                methodAccessorAssignStmts.Add(assignStmt);
            }
            return new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), accessor), "Invoke", 
                targetObject, new CodeArrayCreateExpression(new CodeTypeReference(typeof(object)), args));
        }

        private CodeExpression ResolveSymcall(SymbolicMethodCall smc, IMethod m, SymexState s)
        {
            List<CodeExpression> args = new List<CodeExpression>();
            for (int i = 0, n = smc.args.Count; i < n; ++i)
            {
                args.Add(Compile(smc.args[i], m, s));
            }
            if (smc.method.IsStatic)
            {
                CodeExpression[] argVals = new CodeExpression[smc.args.Count];
                for (int i = 0; i < args.Count; ++i)
                {
                    argVals[i] = args[i];
                }
                return MethodInvoke(smc.method, new CodePrimitiveExpression(null), argVals);
            } else
            {
                CodeExpression thisVal = args[0];
                CodeExpression[] argVals = new CodeExpression[args.Count - 1];
                for (int i = 1; i < args.Count; ++i)
                {
                    argVals[i - 1] = args[i];
                }
                return MethodInvoke(smc.method, thisVal, argVals);
            }
        }

        private CodeExpression ResolveVariable(string varName, IMethod m, SymexState s)
        {
            bool staticField;
            if ((staticField = varName.StartsWith("staticfield:")) || varName.StartsWith("frame:0:this:instancefield:"))
            {
                List<IField> fieldAccesses = new List<IField>();
                string[] parts = varName.Split(':');
                int idx;
                if (staticField)
                {
                    string field = parts[1];
                    int dotIndex = field.LastIndexOf('.');
                    string typeName = field.Substring(0, dotIndex);
                    string fieldName = field.Substring(dotIndex + 1);
                    IType type = Helpers.FindType(SymexMachine.Instance.CSD, typeName);
                    IField f = type.GetFields(f => f.Name == fieldName).First();
                    fieldAccesses.Add(f);
                    idx = 2;
                } else
                {
                    string fieldName = parts[4];
                    IType instanceType = m.DeclaringType;
                    IField f = instanceType.GetFields(f => f.Name == fieldName).First();
                    fieldAccesses.Add(f);
                    idx = 5;
                }
                while (idx < parts.Length)
                {
                    var lastField = fieldAccesses[fieldAccesses.Count - 1];
                    if (parts[idx] == "instancefield")
                    {
                        string fieldName = parts[idx + 1];
                        IType type = lastField.Type;
                        IField f = type.GetFields(f => f.Name == fieldName).First();
                        fieldAccesses.Add(f);
                        idx += 2;
                    }
                    else
                    {
                        throw new Exception("expected instancefield, got " + parts[idx]);
                    }
                }
                CodeExpression value = new CodeVariableReferenceExpression("instance");
                foreach (IField f in fieldAccesses)
                {
                    value = FieldAccess(value, f);
                }
                return value;
            } else if (varName.StartsWith("symcall:"))
            {
                int symcallId = int.Parse(varName.Substring(8));
                return ResolveSymcall(s.symbolicMethodCalls[symcallId], m, s);
            } else
            {
                throw new ResolutionException("cannot resolve variable " + varName);
            }
        }

        private CodeExpression Compile(Expr expr, IMethod m, SymexState s)
        {
            switch (expr.FuncDecl.DeclKind)
            {
                case Z3_decl_kind.Z3_OP_NOT:
                    {
                        return new CodeBinaryOperatorExpression(
                            Compile(expr.Arg(0), m, s), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(false));
                    }
                case Z3_decl_kind.Z3_OP_AND:
                    {
                        if (expr.NumArgs > 2)
                        {
                            throw new ResolutionException("more than 2 args not supported in and expression");
                        }
                        return new CodeBinaryOperatorExpression(
                            Compile(expr.Arg(0), m, s), CodeBinaryOperatorType.BooleanAnd, Compile(expr.Arg(1), m, s));
                    }
                case Z3_decl_kind.Z3_OP_OR:
                    {
                        if (expr.NumArgs > 2)
                        {
                            throw new ResolutionException("more than 2 args not supported in or expression");
                        }
                        return new CodeBinaryOperatorExpression(
                            Compile(expr.Arg(0), m, s), CodeBinaryOperatorType.BooleanOr, Compile(expr.Arg(1), m, s));
                    }
                case Z3_decl_kind.Z3_OP_EQ:
                    {
                        return new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("CompileHelpers"), "Equals", Compile(expr.Arg(0), m, s), Compile(expr.Arg(1), m, s));
                    }
                case Z3_decl_kind.Z3_OP_FPA_EQ:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.IdentityEquality,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_ITE:
                    {
                        var cond = Compile(expr.Arg(0), m, s);
                        var trueVal = Compile(expr.Arg(1), m, s);
                        var falseVal = Compile(expr.Arg(2), m, s);
                        return new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("CompileHelpers"), "IfThenElse", cond, trueVal, falseVal);
                    }
                case Z3_decl_kind.Z3_OP_FPA_GT:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.GreaterThan,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_GE:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.GreaterThanOrEqual,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_LT:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.LessThan,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_LE:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.LessThanOrEqual,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_IS_NAN:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(double)), "IsNaN", val1);
                    }
                case Z3_decl_kind.Z3_OP_BUREM:
                case Z3_decl_kind.Z3_OP_BSREM:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.Modulus,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_REM:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.Modulus,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_BAND:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.BitwiseAnd,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_BOR:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.BitwiseOr,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_BXOR:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("CompileHelpers"), "Xor",
                                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_BSHL:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("CompileHelpers"), "Shl",
                                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_BASHR:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("CompileHelpers"), "Shr",
                                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_BADD:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.Add,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_ULT:
                case Z3_decl_kind.Z3_OP_SLT:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.LessThan,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_ULEQ:
                case Z3_decl_kind.Z3_OP_LE:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.LessThanOrEqual,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_UGT:
                case Z3_decl_kind.Z3_OP_GT:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.GreaterThan,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_UGEQ:
                case Z3_decl_kind.Z3_OP_GE:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.GreaterThanOrEqual,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_ADD:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.Add,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_BSUB:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.Subtract,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_SUB:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.Subtract,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_BMUL:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.Multiply,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_MUL:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.Multiply,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_BUDIV:
                case Z3_decl_kind.Z3_OP_BSDIV:
                    {
                        var val1 = Compile(expr.Arg(0), m, s);
                        var val2 = Compile(expr.Arg(1), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val1),
                            CodeBinaryOperatorType.Divide,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val2));
                    }
                case Z3_decl_kind.Z3_OP_FPA_DIV:
                    {
                        var val1 = Compile(expr.Arg(expr.NumArgs == 3 ? 1u : 0u), m, s);
                        var val2 = Compile(expr.Arg(expr.NumArgs == 3 ? 2u : 1u), m, s);
                        return new CodeBinaryOperatorExpression(
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val1),
                            CodeBinaryOperatorType.Divide,
                            new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToDouble", val2));
                    }
                case Z3_decl_kind.Z3_OP_CONCAT:
                    {
                        // concat only used to extend a value with 0s, so ignore the first argument
                        var val = Compile(expr.Arg(1), m, s);
                        return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val);
                    }
                case Z3_decl_kind.Z3_OP_EXTRACT:
                    {
                        var val = Compile(expr.Arg(1), m, s);
                        return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val);
                    }
                case Z3_decl_kind.Z3_OP_FPA_TO_FP:
                case Z3_decl_kind.Z3_OP_FPA_TO_FP_UNSIGNED:
                    {
                        return Compile(expr.Arg(1), m, s);
                    }
                case Z3_decl_kind.Z3_OP_FPA_TO_SBV:
                case Z3_decl_kind.Z3_OP_FPA_TO_UBV:
                    {
                        var val = Compile(expr.Arg(1), m, s);
                        return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("CompileHelpers"), "ToUlong", val);
                    }
                case Z3_decl_kind.Z3_OP_BNUM:
                    {
                        var val = ulong.Parse(expr.ToString());
                        return new CodePrimitiveExpression(val);
                    }
                case Z3_decl_kind.Z3_OP_FPA_PLUS_INF:
                    {
                        return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(double)), "PositiveInfinity");
                    }
                case Z3_decl_kind.Z3_OP_FPA_MINUS_INF:
                    {
                        return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(double)), "NegativeInfinity");
                    }
                case Z3_decl_kind.Z3_OP_FPA_PLUS_ZERO:
                case Z3_decl_kind.Z3_OP_FPA_MINUS_ZERO:
                    {
                        return new CodePrimitiveExpression(0.0);
                    }
                case Z3_decl_kind.Z3_OP_FPA_NUM:
                    {
                        var val = double.Parse(expr.ToString());
                        return new CodePrimitiveExpression(val);
                    }
                case Z3_decl_kind.Z3_OP_TRUE:
                    {
                        return new CodePrimitiveExpression(true);
                    }
                case Z3_decl_kind.Z3_OP_FALSE:
                    {
                        return new CodePrimitiveExpression(false);
                    }
                case Z3_decl_kind.Z3_OP_UNINTERPRETED:
                    {
                        return ResolveVariable(expr.FuncDecl.Name.ToString(), m, s);
                    }
                case Z3_decl_kind.Z3_OP_ANUM:
                    throw new ResolutionException("object resolution not yet supported");
                default:
                    throw new ResolutionException("unsupported expr: " + expr + " (kind " + expr.FuncDecl.DeclKind + ")");
            }
        }

        private CodeExpression And(List<CodeExpression> predicates)
        {
            if (predicates.Count == 0)
            {
                return new CodePrimitiveExpression(true);
            } else
            {
                CodeExpression current = predicates[0];
                for (int i = 1; i < predicates.Count; ++i)
                {
                    current = new CodeBinaryOperatorExpression(current, CodeBinaryOperatorType.BooleanAnd, predicates[i]);
                }
                return current;
            }
        }

        public void ProcessMethod(IMethod method, SymexMachine m)
        {
            PrettyPrint pp = new PrettyPrint(m, m.Z3);
            pathFuncAssignStmts.Add(new CodeAssignStatement(
                   new CodeVariableReferenceExpression("pathFuncs"), new CodeObjectCreateExpression("List<Func<MonoBehaviour, bool>>")));
            int pathId = 1;
            foreach (SymexState s in m.States)
            {
                List<BoolExpr> preconditions = new List<BoolExpr>();
                List<CodeExpression> predicates = new List<CodeExpression>();
                foreach (BoolExpr cond in s.pathCondition)
                {
                    if (Helpers.ContainsInputVariable(cond, s))
                    {
                        continue;
                    }
                    preconditions.Add(cond);

                    try
                    {
                        predicates.Add(Compile(cond, method, s));
                    } catch (ResolutionException e)
                    {
                        // Console.WriteLine("warning: resolution error for '" + cond + "': " + e.Message);
                    }
                }
                CodeExpression compiledPathCond = And(predicates);

                string methodName = method.FullName.Replace('.', '_') + "_Path" + pathId;
                CodeMemberMethod codeMethod = new CodeMemberMethod();
                codeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                codeMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("MonoBehaviour"), "instance"));
                codeMethod.Name = methodName;
                codeMethod.ReturnType = new CodeTypeReference(typeof(bool));
                codeMethod.Statements.Add(new CodeMethodReturnStatement(compiledPathCond));
                StringWriter sw = new StringWriter();
                pp.WritePrettyPathCondition(s, sw);
                codeMethod.Comments.Add(new CodeCommentStatement(sw.ToString()));
                pathMethods.Add(codeMethod);

                pathFuncAssignStmts.Add(new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("pathFuncs"), "Add",
                        new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), methodName))));

                ++pathId;
            }

            pathFuncAssignStmts.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "preconditionFuncs"),
                "Add",
                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("SymexHelpers"),
                    "GetMethodFromSignature", new CodePrimitiveExpression(Helpers.GetMethodSignature(method))),
                new CodeVariableReferenceExpression("pathFuncs"))));
        }

        public void Finish()
        {
            foreach (CodeMemberField fieldAccessorField in fieldAccessorFields)
            {
                pfuncsClass.Members.Add(fieldAccessorField);
            }
            foreach (CodeMemberField methodAccessorField in methodAccessorFields)
            {
                pfuncsClass.Members.Add(methodAccessorField);
            }

            CodeMemberField preconditionFuncs = new CodeMemberField();
            preconditionFuncs.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            preconditionFuncs.Name = "preconditionFuncs";
            preconditionFuncs.Type = new CodeTypeReference("Dictionary<MethodInfo, List<Func<MonoBehaviour, bool>>>");
            pfuncsClass.Members.Add(preconditionFuncs);

            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("BindingFlags"), "bindingFlags",
                new CodeBinaryOperatorExpression(
                    new CodeBinaryOperatorExpression(
                        new CodeBinaryOperatorExpression(
                            new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("BindingFlags"), "Public"), CodeBinaryOperatorType.BitwiseOr,
                            new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("BindingFlags"), "NonPublic")),
                        CodeBinaryOperatorType.BitwiseOr,
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("BindingFlags"), "Instance")),
                    CodeBinaryOperatorType.BitwiseOr,
                    new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("BindingFlags"), "Static"))));
            foreach (CodeStatement assignStmt in fieldAccessorAssignStmts)
            {
                ctor.Statements.Add(assignStmt);
            }
            foreach (CodeStatement assignStmt in methodAccessorAssignStmts)
            {
                ctor.Statements.Add(assignStmt);
            }
            ctor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "preconditionFuncs"),
                new CodeObjectCreateExpression("Dictionary<MethodInfo, List<Func<MonoBehaviour, bool>>>")));
            ctor.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("List<Func<MonoBehaviour, bool>>"), "pathFuncs"));
            foreach (CodeStatement assignStmt in pathFuncAssignStmts)
            {
                ctor.Statements.Add(assignStmt);
            }
            pfuncsClass.Members.Add(ctor);

            foreach (CodeMemberMethod pathMethod in pathMethods)
            {
                pfuncsClass.Members.Add(pathMethod);
            }
        }

        public void Write(StreamWriter sw)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            provider.GenerateCodeFromCompileUnit(pfuncsUnit, sw, options);
        }
    }
}