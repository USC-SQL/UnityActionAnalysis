using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace UnityActionAnalysis
{
    public class InputInstrumentation
    {
        public static bool IsInstrumented(string assemblyFileName, GameConfiguration config)
        {
            using (ModuleDefinition module = ModuleDefinition.ReadModule(assemblyFileName))
            {
                foreach (TypeDefinition type in module.Types)
                {
                    if (type.Namespace == "UnityActionAnalysis" || config.IsNamespaceIgnored(type.Namespace))
                    {
                        continue;
                    }
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.Body == null)
                        {
                            continue;
                        }
                        foreach (Instruction inst in method.Body.Instructions)
                        {
                            if (inst.OpCode.Name == "call" && inst.Operand is MethodReference methodRef)
                            {
                                if (methodRef.DeclaringType.FullName == "UnityActionAnalysis.InstrInput")
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }

        private static string GetSubsignature(MethodReference method)
        {
            return method.Name + "(" + string.Join(",", method.Parameters.Select(param => param.ParameterType.FullName)) + ")";
        }

        private static void InstrumentMethod(MethodDefinition method, Dictionary<string, MethodReference> instrMethods)
        {
            if (method.Body == null)
            {
                return;
            }
            foreach (Instruction inst in method.Body.Instructions)
            {
                if (inst.OpCode.Name == "call" && inst.Operand is MethodReference methodRef
                    && methodRef.DeclaringType.FullName == "UnityEngine.Input")
                {
                    string subsig = GetSubsignature(methodRef);
                    if (instrMethods.TryGetValue(subsig, out MethodReference instrMethod))
                    {
                        inst.Operand = instrMethod;
                    }
                }
            }
        }

        public static void Run(GameConfiguration config)
        {
            Console.WriteLine("Instrumenting input API calls of " + config.assemblyFileName);

            if (IsInstrumented(config.assemblyFileName, config))
            {
                Console.WriteLine("Assembly is already instrumented, skipping");
                return;
            }

            string instrInputTypeName = "UnityActionAnalysis.InstrInput";
            ModuleDefinition module = ModuleDefinition.ReadModule(config.assemblyFileName);
            TypeDefinition instrInput = module.GetType(instrInputTypeName);
            if (instrInput == null)
            {
                throw new Exception("Could not find " + instrInputTypeName);
            }
            Dictionary<string, MethodReference> instrMethods = new Dictionary<string, MethodReference>();
            foreach (MethodDefinition method in instrInput.Methods)
            {
                if (method.IsPublic && method.IsStatic)
                {
                    instrMethods.Add(GetSubsignature(method), method);
                }
            }

            foreach (TypeDefinition type in module.Types)
            {
                if (type.Namespace == "UnityActionAnalysis" || config.IsNamespaceIgnored(type.Namespace))
                {
                    continue;
                }
                foreach (MethodDefinition method in type.Methods)
                {
                    InstrumentMethod(method, instrMethods);
                }
            }

            module.Write(config.assemblyFileName + ".tmp");
            module.Dispose();
            File.Move(config.assemblyFileName, config.assemblyFileName + ".orig");
            File.Move(config.assemblyFileName + ".tmp", config.assemblyFileName);
        }

    }
}
