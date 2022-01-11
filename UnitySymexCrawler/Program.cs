using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class UnityConfiguration : Configuration
    {
        public override bool IsMethodSymbolic(IMethod method)
        {
            return method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule;
        }

        private static bool IsInputAPI(IMethod method)
        {
            return method.DeclaringType.FullName == "UnityEngine.Input" && method.Parameters.Count == 1;
        }

        public override int SymbolicMethodResultVarId(IMethod method, List<Expr> arguments, SymexState state)
        {
            if (IsInputAPI(method))
            {
                string arg = JsonSerializer.Serialize(state.SerializeExpr(arguments[0]));
                foreach (var p in state.symbolicMethodCalls)
                {
                    SymbolicMethodCall smc = p.Value;
                    if (IsInputAPI(smc.method) && JsonSerializer.Serialize(state.SerializeExpr(smc.args[0])) == arg)
                    {
                        return p.Key;
                    }
                }
            }
            return base.SymbolicMethodResultVarId(method, arguments, state);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            SymexMachine.SetUpGlobals();

            var assemblyFileName =
                @"C:\Users\sasha-usc\Misc\UnitySymexCrawler\Assembly-CSharp.dll";
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
            assemblyResolver.AddSearchDirectory(@"C:\Users\sasha-usc\Documents\AutoExplore\SymexExperiments\Pacman\Assets\Packages\Microsoft.Z3.x64.4.8.10\lib\netstandard1.4");
            assemblyResolver.AddSearchDirectory(@"C:\Users\sasha-usc\Documents\AutoExplore\SymexExperiments\Pacman\Assets\Packages\Microsoft.Data.Sqlite.Core.6.0.1\lib\netstandard2.0");
            var settings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(peFile, assemblyResolver, settings);
            var ua = new UnityAnalysis(decompiler);

            List<IMethod> targets = new List<IMethod>();
            foreach (IMethod method in ua.FindMonoBehaviourMethods(m => (m.Name == "Update" || m.Name == "FixedUpdate" || m.Name == "LateUpdate") && m.Parameters.Count == 0))
            {
                bool diia = ua.DoesInvokeInputAPI(method);
                if (diia)
                {
                    targets.Add(method);
                }
            }

            var databaseFile = "symex.db";
            if (File.Exists(databaseFile))
            {
                File.Delete(databaseFile);
            }
            using (var db = new DatabaseUtil(databaseFile))
            {
                using (var z3 = new Context(new Dictionary<string, string>() {{"model", "true"}}))
                {
                    foreach (IMethod method in targets)
                    {
                        Console.WriteLine("Processing " + method.FullName + "(" + string.Join(",", method.Parameters.Select(param => param.Type.FullName)) + ")");
                        SymexMachine m = new SymexMachine(decompiler, method, new UnityConfiguration());
                        m.Run();
                        db.AddPaths(method, m);
                        m.Dispose();
                    }
                }
            }
        }
    }
}
