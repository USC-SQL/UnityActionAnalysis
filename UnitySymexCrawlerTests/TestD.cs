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
    public class TestD
    {
        private static int Calc(int id)
        {
            int y = id;
            for (int i = 0; i < 5; ++i)
            {
                y += i * y;
            }
            return y;
        }

        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.TestD.ProgramD", "Main", new TestConfig()))
            {  
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(4, machine.States.Count);
                Assert.AreEqual(4, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var arg1 = z3.MkConst("frame:0:arg:1", z3.MkBitVecSort(32));
                    var arg3 = z3.MkConst("frame:0:arg:3", z3.MkBitVecSort(32));
                    var p1FavoriteColor = z3.MkConst("frame:5:this:instancefield:<P1FavoriteColor>k__BackingField", z3.MkBitVecSort(32));
                    var p2FavoriteColor = z3.MkConst("frame:7:this:instancefield:<P2FavoriteColor>k__BackingField", z3.MkBitVecSort(32));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        Console.WriteLine(m);
                        if (TestHelpers.ModelContainsVariables(m, arg1, arg3, p1FavoriteColor, p2FavoriteColor))
                        {
                            int id1 = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int id2 = (int)uint.Parse(m.Evaluate(arg3).ToString());
                            int p1fc = (int)uint.Parse(m.Evaluate(p1FavoriteColor).ToString());
                            int p2fc = (int)uint.Parse(m.Evaluate(p2FavoriteColor).ToString());
                            return id1 > id2 && id1 == Calc(id2) && p1fc == p2fc + 1;
                        } else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg1, arg3, p1FavoriteColor, p2FavoriteColor))
                        {
                            int id1 = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int id2 = (int)uint.Parse(m.Evaluate(arg3).ToString());
                            int p1fc = (int)uint.Parse(m.Evaluate(p1FavoriteColor).ToString());
                            int p2fc = (int)uint.Parse(m.Evaluate(p2FavoriteColor).ToString());
                            return id1 > id2 && id1 == Calc(id2) && p1fc != p2fc + 1;
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg1, arg3))
                        {
                            int id1 = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int id2 = (int)uint.Parse(m.Evaluate(arg3).ToString());
                            return id1 > id2 && id1 != Calc(id2);
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg1, arg3))
                        {
                            int id1 = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int id2 = (int)uint.Parse(m.Evaluate(arg3).ToString());
                            return id1 <= id2;
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
