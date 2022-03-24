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
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GameConfiguration gameConfig = GameConfigs.GAME_CONFIG_SMW;

            SymexMachine.SetUpGlobals();

            var assemblyFileName = gameConfig.assemblyFileName;

            var peFile = new PEFile(assemblyFileName,
                new FileStream(assemblyFileName, FileMode.Open, FileAccess.Read),
                streamOptions: PEStreamOptions.PrefetchEntireImage);
            var assemblyResolver = new UniversalAssemblyResolver(assemblyFileName, true,
                peFile.DetectTargetFrameworkId(),
                peFile.DetectRuntimePack(),
                PEStreamOptions.PrefetchEntireImage,
                MetadataReaderOptions.None);
            foreach (string searchDir in GameConfigs.BASE_SEARCH_DIRECTORIES)
            {
                assemblyResolver.AddSearchDirectory(searchDir);
            }
            foreach (string searchDir in gameConfig.assemblySearchDirectories)
            {
                assemblyResolver.AddSearchDirectory(searchDir);
            }

            var settings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(peFile, assemblyResolver, settings);
            var ua = new UnityAnalysis(decompiler);

            List<IMethod> targets = new List<IMethod>();
            foreach (IMethod method in ua.FindMonoBehaviourMethods(m => (m.Name == "Update" || m.Name == "FixedUpdate" || m.Name == "LateUpdate") && m.Parameters.Count == 0))
            {
                if (ua.DoesInvokeInputAPI(method))
                {
                    targets.Add(method);
                }
            }

            var databaseFile = gameConfig.outputDatabase;
            if (File.Exists(databaseFile))
            {
                File.Delete(databaseFile);
            }

            using var db = new DatabaseUtil(databaseFile);
            using var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } });

            string pfuncsFile = gameConfig.outputPrecondFuncs;
            if (File.Exists(pfuncsFile))
            {
                File.Delete(pfuncsFile);
            }

            PreconditionFuncsGen pfg = new PreconditionFuncsGen();

            using var prettyLog = new StreamWriter(File.OpenWrite("log.txt"));
            foreach (IMethod method in targets)
            {
                Console.WriteLine("Processing " + method.FullName + "(" + string.Join(",", method.Parameters.Select(param => param.Type.FullName)) + ")");
                MethodPool methodPool = new MethodPool();
                Console.WriteLine("\tAnalyzing branches");
                InputBranchAnalysis iba = new InputBranchAnalysis(method, methodPool);
                var ibaResult = iba.Perform();
                SymexMachine m = new SymexMachine(decompiler, method, methodPool, new UnityConfiguration(ibaResult));
                Console.WriteLine("\tRunning symbolic execution");
                m.Run();
                Console.WriteLine("\tWriting path information to database");
                db.AddPaths(method, m);
                Console.WriteLine("\tGenerating code");
                pfg.ProcessMethod(method, m);
                m.Dispose();
            }

            pfg.Finish();

            using var pfuncsOut = new StreamWriter(File.OpenWrite(pfuncsFile));
            pfg.Write(pfuncsOut);
        }
    }
}
