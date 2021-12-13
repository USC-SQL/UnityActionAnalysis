using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexActionIdentification.Tests
{
    public class ConfigG : TestConfig
    {
        public override bool IsMethodSymbolic(IMethod method)
        {
            return base.IsMethodSymbolic(method) || method.Name == "FetchRecordFromDB";
        }
    }

    [TestClass()]
    public class TestG
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.TestG.ProgramG", "Main", new ConfigG()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);
                    helper.ForAllStates((s, m) =>
                    {
                        Console.WriteLine(m);
                        Console.WriteLine("--");
                        return true;
                    });
                }
            }
        }
    }
}
