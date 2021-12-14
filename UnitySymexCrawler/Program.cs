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
    public class UnityConfiguration : Configuration
    {
        public override bool IsMethodSymbolic(IMethod method)
        {
            return method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            SymexMachine.SetUpGlobals();

            var assemblyFileName =
                @"C:\Users\sasha-usc\Documents\AutoExplore\SymexExperiments\Pacman\Library\ScriptAssemblies\Assembly-CSharp.dll";
            var peFile = new PEFile(assemblyFileName,
                new FileStream(assemblyFileName, FileMode.Open, FileAccess.Read),
                streamOptions: PEStreamOptions.PrefetchEntireImage);
            var assemblyResolver = new UniversalAssemblyResolver(assemblyFileName, true,
                peFile.DetectTargetFrameworkId(),
                peFile.DetectRuntimePack(),
                PEStreamOptions.PrefetchMetadata,
                MetadataReaderOptions.None);
            assemblyResolver.AddSearchDirectory(@"C:\Program Files\Unity\Hub\Editor\2020.3.17f1\Editor\Data\Managed\UnityEngine");
            assemblyResolver.AddSearchDirectory(@"C:\Users\sasha-usc\Documents\AutoExplore\SymexExperiments\Pacman\Library\ScriptAssemblies");
            var settings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(peFile, assemblyResolver, settings);
            var name = new FullTypeName("PlayerController");
            var playerController = decompiler.TypeSystem.MainModule.Compilation.FindType(name);
            var fixedUpdate = playerController.GetMethods().Where(m => m.FullNameIs("PlayerController", "FixedUpdate")).First();

            SymexMachine m = new SymexMachine(decompiler, fixedUpdate, new UnityConfiguration());
            m.Run();

            foreach (SymexState s in m.States)
            {
                Console.WriteLine(s.PathConditionString());

                foreach (var p in s.symbolicMethodCalls)
                {
                    Console.WriteLine("symcall:" + p.Key + ": " + p.Value.method.FullName + "(" + string.Join(",", p.Value.args) + ")");
                }

                Console.WriteLine("--");
            }
        }
    }
}
