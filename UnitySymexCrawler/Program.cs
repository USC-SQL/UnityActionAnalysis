﻿using System;
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
    public struct GameConfiguration
    {
        public string assemblyFileName;
        public List<string> assemblySearchDirectories;

        public GameConfiguration(string assemblyFileName, List<string> assemblySearchDirs)
        {
            this.assemblyFileName = assemblyFileName;
            assemblySearchDirectories = assemblySearchDirs;
        }
    }

    public class Program
    {
        public static readonly GameConfiguration TETRIS_GAME_CONFIG =
            new GameConfiguration(@"C:\Users\Sasha Volokh\Misc\UnityTetris\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>()
                {
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Library\ScriptAssemblies",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\External\Demigiant\DOTween",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\Scripts\SymexCrawler\Packages\InputSimulator.1.0.4\lib\net20",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\Scripts\SymexCrawler\Packages\Microsoft.Z3.x64.4.8.10\lib\netstandard1.4",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Assets\Scripts\SymexCrawler\Packages\Microsoft.Data.Sqlite.Core.6.0.1\lib\netstandard2.0",
                    @"C:\Users\Sasha Volokh\Misc\UnityTetris\Library\PackageCache\com.unity.nuget.newtonsoft-json@2.0.0\Runtime"
                });

        public static readonly GameConfiguration PACMAN_GAME_CONFIG =
            new GameConfiguration(@"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Library\ScriptAssemblies\Assembly-CSharp.dll",
                new List<string>() {
                    @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Library\ScriptAssemblies",
                    @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Assets\Scripts\SymexCrawler\Packages\InputSimulator.1.0.4\lib\net20",
                    @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Assets\Scripts\SymexCrawler\Packages\Microsoft.Z3.x64.4.8.10\lib\netstandard1.4",
                    @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Assets\Scripts\SymexCrawler\Packages\Microsoft.Data.Sqlite.Core.6.0.1\lib\netstandard2.0",
                    @"C:\Users\Sasha Volokh\Misc\AutoExplore\SymexExperiments\Pacman\Library\PackageCache\com.unity.nuget.newtonsoft-json@2.0.0\Runtime"
                });

        public static void Main(string[] args)
        {
            GameConfiguration gameConfig = TETRIS_GAME_CONFIG;

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

            assemblyResolver.AddSearchDirectory(@"C:\Program Files\Unity\Hub\Editor\2020.3.28f1\Editor\Data\Managed\UnityEngine");
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

            var databaseFile = "symex.db";
            if (File.Exists(databaseFile))
            {
                File.Delete(databaseFile);
            }

            using var db = new DatabaseUtil(databaseFile);
            using var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } });

            string pfuncsFile = "PreconditionFuncs.cs";
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
