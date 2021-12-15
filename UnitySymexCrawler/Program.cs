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

            foreach (IMethod method in targets)
            {
                Console.WriteLine("running symex on " + method);
                SymexMachine m = new SymexMachine(decompiler, method, new UnityConfiguration());
                m.Run();

                using (Context z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    foreach (SymexState s in m.States)
                    {
                        Solver solver = z3.MkSolver();
                        solver.Assert(z3.ParseSMTLIB2String(s.PathConditionString()));
                        solver.Check();
                        Console.WriteLine(solver.Model);

                        foreach (var p in s.symbolicMethodCalls)
                        {
                            List<string> methodArgs = new List<string>();
                            foreach (Expr arg in p.Value.args)
                            {
                                methodArgs.Add(JsonSerializer.Serialize(s.SerializeExpr(arg)));
                            }
                            Console.WriteLine("symcall:" + p.Key + ": " + p.Value.method + "(" + string.Join(",", methodArgs) + ")");
                        }

                        Console.WriteLine("---");
                    }
                }

                Console.WriteLine("obtained " + m.States.Count + " states");

                m.Dispose();
            }
        }
    }
}
