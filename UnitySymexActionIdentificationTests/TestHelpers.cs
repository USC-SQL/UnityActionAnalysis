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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ICSharpCode.Decompiler.IL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

namespace UnitySymexActionIdentification.Tests
{
    public class TestHelpers
    {
        public static SymexMachine CreateMachine(string entryPointClassFullName, string entryPointMethodName, Configuration config)
        {
            string assemblyPath = @"..\..\..\..\TestCases\bin\Debug\netcoreapp3.1\TestCases.dll";
            var peFile = new PEFile(assemblyPath,
                new FileStream(assemblyPath, FileMode.Open, FileAccess.Read),
                streamOptions: PEStreamOptions.PrefetchEntireImage);
            var assemblyResolver = new UniversalAssemblyResolver(assemblyPath, true,
                peFile.DetectTargetFrameworkId(),
                peFile.DetectRuntimePack(),
                PEStreamOptions.PrefetchMetadata,
                MetadataReaderOptions.None);
            var settings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(peFile, assemblyResolver, settings);
            IType programA = decompiler.TypeSystem.MainModule.Compilation.FindType(new FullTypeName(entryPointClassFullName));
            IMethod main = programA.GetMethods().Where(m => m.Name == entryPointMethodName).First();
            var machine = new SymexMachine(decompiler, main, config);
            return machine;
        }

        public class SymexMachineHelper
        {
            private List<(SymexState, Model)> solved;

            public SymexMachineHelper(SymexMachine m, Context z3)
            {
                solved = new List<(SymexState, Model)>();

                foreach (SymexState s in m.States)
                {
                    Solver solver = z3.MkSolver();
                    solver.Assert(z3.ParseSMTLIB2String(s.PathConditionString()));
                    Assert.AreEqual(Status.SATISFIABLE, solver.Check());
                    solved.Add((s, solver.Model));
                }
            }

            public void AssertExistsPathConditionWhere(Predicate<Model> pred)
            {
                bool exists = false;
                foreach (var p in solved)
                {
                    if (pred(p.Item2))
                    {
                        exists = true;
                        break;
                    }
                }
                Assert.IsTrue(exists);
            }
        }

        public static bool ModelContainsVariables(Model m, params string[] varNames)
        {
            HashSet<string> remaining = new HashSet<string>(varNames);
            foreach (FuncDecl decl in m.Decls)
            {
                remaining.Remove(decl.Name.ToString());
            }
            return remaining.Count() == 0;
        }

        public static void CommonAssertionsAfterRun(SymexMachine machine)
        {
            foreach (SymexState state in machine.States)
            {
                if (state.execStatus == ExecutionStatus.HALTED)
                {
                    Assert.AreEqual(0, state.opQueue.Count);
                }
            }
        }
    }
}
