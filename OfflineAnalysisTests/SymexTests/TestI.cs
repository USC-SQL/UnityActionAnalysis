using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnityActionAnalysis.Tests
{
    public class ConfigI : TestConfig
    {
        public override bool IsMethodSummarized(IMethod method)
        {
            return base.IsMethodSummarized(method) || method.FullName == "OfflineAnalysisTestCases.Symex.TestI.Record..ctor";
        }
    }

    [TestClass()]
    public class TestI
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("OfflineAnalysisTestCases.Symex.TestI.ProgramI", "Main", new ConfigI()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(30, machine.States.Count);
                Assert.AreEqual(30, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var r1x = z3.MkConst("symcall:0:instancefield:x", z3.MkBitVecSort(32));
                    var r2x = z3.MkConst("symcall:1:instancefield:x", z3.MkBitVecSort(32));

                    Func<int, bool>[] cases1 = new Func<int, bool>[]
                    {
                        (int x1) => x1 == 1,
                        (int x1) => x1 == 2,
                        (int x1) => x1 == 3,
                        (int x1) => x1 == 4,
                        (int x1) => x1 == 5,
                        (int x1) => x1 == 6,
                        (int x1) => x1 == 7,
                        (int x1) => x1 == 8,
                        (int x1) => x1 == 9,
                        (int x1) => x1 == 10,
                        (int x1) => x1 == 11,
                        (int x1) => x1 == 12,
                        (int x1) => x1 == 13,
                        (int x1) => x1 == 14,
                        (int x1) => x1 == 15,
                        (int x1) => x1 == 16,
                        (int x1) => x1 == 17,
                        (int x1) => x1 == 18,
                        (int x1) => x1 == 19,
                        (int x1) => x1 == 20,
                        (int x1) => x1 == 21,
                        (int x1) => x1 < 1 || x1 > 21,
                    };

                    Func<int, int, bool>[] cases2 = new Func<int, int, bool>[]
                    {
                        (int x1, int x2) => x1 == 1 && x2 == 1,
                        (int x1, int x2) => x1 == 1 && x2 == 2,
                        (int x1, int x2) => x1 == 1 && x2 == 3,
                        (int x1, int x2) => x1 == 1 && x2 == 4,
                        (int x1, int x2) => x1 == 1 && x2 == 5,
                        (int x1, int x2) => x1 == 1 && x2 == 6,
                        (int x1, int x2) => x1 == 1 && x2 == 7,
                        (int x1, int x2) => x1 == 1 && x2 == 8,
                        (int x1, int x2) => x1 == 1 && (x2 < 1 || x2 > 8)
                    };

                    int caseIndex = 0;
                    foreach (var c in cases1)
                    {
                        Assert.IsTrue(helper.ExistsState((s, m) =>
                        {
                            if (TestHelpers.ModelContainsVariables(m, r1x))
                            {
                                int x1 = (int)uint.Parse(m.Evaluate(r1x).ToString());
                                return c.Invoke(x1);
                            }
                            else
                            {
                                return false;
                            }
                        }), "cases1[" + caseIndex + "]");
                        ++caseIndex;
                    }

                    caseIndex = 0;
                    foreach (var c in cases2)
                    {
                        Assert.IsTrue(helper.ExistsState((s, m) =>
                        {
                            if (TestHelpers.ModelContainsVariables(m, r1x, r2x))
                            {
                                int x1 = (int)uint.Parse(m.Evaluate(r1x).ToString());
                                int x2 = (int)uint.Parse(m.Evaluate(r2x).ToString());
                                return c.Invoke(x1, x2);
                            }
                            else
                            {
                                return false;
                            }
                        }), "cases2[" + caseIndex + "]");
                        ++caseIndex;
                    }
                }
            }
        }
    }
}
