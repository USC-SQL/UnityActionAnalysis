using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Diagnostics;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler
{
    public class UnityAnalysis
    {
        private CSharpDecompiler csd;
        private Dictionary<IMethod, ILFunction> ilFunctions;

        public UnityAnalysis(CSharpDecompiler csd)
        {
            this.csd = csd;
            ilFunctions = new Dictionary<IMethod, ILFunction>();
        }

        private ILFunction ReadMethod(IMethod m)
        {
            ILFunction result;
            if (ilFunctions.TryGetValue(m, out result))
            {
                return result;
            } else
            {
                MethodDefinitionHandle methodDefHandle = (MethodDefinitionHandle)m.MetadataToken;
                MetadataModule module = (MetadataModule)m.ParentModule;
                PEFile peFile = module.PEFile;
                MethodDefinition methodDef = peFile.Metadata.GetMethodDefinition(methodDefHandle);
                MethodBodyBlock methodBody = peFile.Reader.GetMethodBody(methodDef.RelativeVirtualAddress);
                ILReader reader = new ILReader(module);
                ILFunction func = reader.ReadIL(methodDefHandle, methodBody);
                ilFunctions.Add(m, func);
                return func;
            }
        }

        private bool CheckCallInstruction(ILInstruction inst, HashSet<IMethod> visited)
        {
            if (inst is CallInstruction)
            {
                CallInstruction callInst = (CallInstruction)inst;
                IMethod target = callInst.Method;
                if (target.DeclaringType.FullName == "UnityEngine.Input")
                {
                    return true;
                }
                else if (!visited.Contains(target) && target.ParentModule == csd.TypeSystem.MainModule)
                {
                    if (DoesInvokeInputAPIInternal(target, visited))
                    {
                        return true;
                    }
                }
            } else 
            {
                foreach (ILInstruction child in inst.Children)
                {
                    if (CheckCallInstruction(child, visited))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool DoesInvokeInputAPIInternal(IMethod m, HashSet<IMethod> visited)
        {
            visited.Add(m);
            ILFunction func = ReadMethod(m);
            BlockContainer bc = (BlockContainer)func.Body;
            foreach (Block b in bc.Blocks)
            {
                foreach (ILInstruction inst in b.Instructions)
                {
                    if (CheckCallInstruction(inst, visited))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // whether this method or any of its callees invoke an Input API
        public bool DoesInvokeInputAPI(IMethod m)
        {
            return DoesInvokeInputAPIInternal(m, new HashSet<IMethod>());
        }

        public ITypeDefinition FindMonoBehaviourType()
        {
            return (ITypeDefinition)csd.TypeSystem.FindType(new FullTypeName("UnityEngine.MonoBehaviour"));
        }

        public IEnumerable<IMethod> FindMonoBehaviourMethods(Predicate<IMethod> mbMethodPredicate)
        {
            ITypeDefinition monoBehaviour = FindMonoBehaviourType();
            foreach (ITypeDefinition type in csd.TypeSystem.MainModule.TypeDefinitions)
            {
                if (type.IsDerivedFrom(monoBehaviour))
                {
                    foreach (IMethod method in type.Methods)
                    {
                        if (mbMethodPredicate(method))
                        {
                            yield return method;
                        }
                    }
                }
            }
            yield break;
        }
    }
}
