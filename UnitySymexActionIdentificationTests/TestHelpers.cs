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

        public delegate bool StatePredicate(SymexState s, Model m);

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

            public bool ExistsState(StatePredicate pred)
            {
                foreach (var p in solved)
                {
                    if (pred(p.Item1, p.Item2))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool ForAllStates(StatePredicate pred)
            {
                foreach (var p in solved)
                {
                    if (!pred(p.Item1, p.Item2))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static bool ModelContainsVariables(Model m, params Expr[] consts)
        {
            HashSet<string> remaining = new HashSet<string>();
            foreach (Expr c in consts)
            {
                remaining.Add(c.ToString());
            }
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
