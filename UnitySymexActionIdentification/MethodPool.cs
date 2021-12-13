using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexActionIdentification
{
    public class MethodPool
    {
        private Dictionary<IMethod, InstructionPointer> entryPoints = new Dictionary<IMethod, InstructionPointer>();

        public InstructionPointer MethodEntryPoint(IMethod method)
        {
            InstructionPointer result;
            if (entryPoints.TryGetValue(method, out result))
            {
                return result;
            } else
            {
                MethodDefinitionHandle methodDefHandle = (MethodDefinitionHandle)method.MetadataToken;
                MetadataModule module = (MetadataModule)method.ParentModule;
                PEFile peFile = module.PEFile;
                MethodDefinition methodDef = peFile.Metadata.GetMethodDefinition(methodDefHandle);
                MethodBodyBlock methodBody = peFile.Reader.GetMethodBody(methodDef.RelativeVirtualAddress);
                ILReader reader = new ILReader(module);
                ILFunction ilFunction = reader.ReadIL(methodDefHandle, methodBody);
                Console.WriteLine(ilFunction);
                BlockContainer bc = (BlockContainer)ilFunction.Body;
                InstructionPointer IP = new InstructionPointer(bc.Blocks[0], 0);
                entryPoints[method] = IP;
                return IP;
            }
        }
    }
}
