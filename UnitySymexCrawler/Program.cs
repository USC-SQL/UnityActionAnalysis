using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var assemblyFileName =
                @"ScriptAssemblies\\Assembly-CSharp.dll";
            var peFile = new PEFile(assemblyFileName,
                new FileStream(assemblyFileName, FileMode.Open, FileAccess.Read),
                streamOptions: PEStreamOptions.PrefetchEntireImage);
            var assemblyResolver = new UniversalAssemblyResolver(assemblyFileName, true,
                peFile.DetectTargetFrameworkId(),
                peFile.DetectRuntimePack(),
                PEStreamOptions.PrefetchMetadata,
                MetadataReaderOptions.None);
            assemblyResolver.AddSearchDirectory(@"C:\Program Files\Unity\Hub\Editor\2020.3.17f1\Editor\Data\Managed\UnityEngine");
            assemblyResolver.AddSearchDirectory(@"C:\Users\sasha-usc\Documents\AutoExplore\IdentifyingActions\Pacman\Library\ScriptAssemblies");
            var settings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(peFile, assemblyResolver, settings);
            var name = new FullTypeName("PlayerController");
            var playerController = decompiler.TypeSystem.MainModule.Compilation.FindType(name);
            var fixedUpdate = playerController.GetMethods().Where(m => m.FullNameIs("PlayerController", "FixedUpdate")).First();
            
            var reader = new ILReader(decompiler.TypeSystem.MainModule);
            var methodDefHandle = (MethodDefinitionHandle)fixedUpdate.MetadataToken;
            var methodDef = peFile.Metadata.GetMethodDefinition(methodDefHandle);
            var methodBody = peFile.Reader.GetMethodBody(methodDef.RelativeVirtualAddress);
            var ilFunction = reader.ReadIL(methodDefHandle, methodBody);

            StLoc inst = (StLoc)((BlockContainer)ilFunction.Body).Blocks[0].Instructions[3];
            var ilv = inst.Variable;
            Console.WriteLine(ilv);
            Console.WriteLine(ilv.GetType());
        }
    }
}
