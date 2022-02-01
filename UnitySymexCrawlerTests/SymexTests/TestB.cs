using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

namespace UnitySymexCrawler.Tests
{

    [TestClass()]
    public class TestB
    {

        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.Symex.TestB.ProgramB", "Main", new TestConfig()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(2, machine.States.Count);
                Assert.AreEqual(1, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());
                Assert.AreEqual(1, machine.States.Where(s => s.execStatus == ExecutionStatus.ABORTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    helper.ForAllStates((s, m) =>
                    {
                        Console.WriteLine(s.PathConditionString());
                        Console.WriteLine(m);
                        Console.WriteLine("--");
                        return true;
                    });

                    var arg0 = z3.MkConst("frame:0:arg:0", z3.MkBitVecSort(32));
                    var arg1 = z3.MkConst("frame:0:arg:1", z3.MkBitVecSort(32));
                    var arg2 = z3.MkConst("frame:0:arg:2", z3.MkBitVecSort(32));
                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg0, arg1, arg2))
                        {
                            int x = (int)uint.Parse(m.Evaluate(arg0).ToString());
                            int y = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int z = (int)uint.Parse(m.Evaluate(arg2).ToString());
                            float vectorSum = ((float)x) + ((float)y) + ((float)z);
                            return vectorSum <= 10000.0f;
                        }
                        else
                        {
                            return false;
                        }
                    }));
                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg0, arg1, arg2))
                        {
                            int x = (int)uint.Parse(m.Evaluate(arg0).ToString());
                            int y = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int z = (int)uint.Parse(m.Evaluate(arg2).ToString());
                            float vectorSum = ((float)x) + ((float)y) + ((float)z);
                            return vectorSum > 10000.0f;
                        }
                        else
                        {
                            return false;
                        }
                    }));
                }
            }
        }
    }
}
