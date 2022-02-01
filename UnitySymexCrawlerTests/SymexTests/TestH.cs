using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexCrawler.Tests
{
    [TestClass()]
    public class TestH
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.Symex.TestH.ProgramH", "Main", new TestConfig()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(21, machine.States.Count);
                Assert.AreEqual(10, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());
                Assert.AreEqual(11, machine.States.Where(s => s.execStatus == ExecutionStatus.ABORTED).Count());
                
                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var runMode = z3.MkConst("frame:0:arg:0:instancefield:runMode", z3.MkBitVecSort(64));
                    var x = z3.MkConst("frame:0:arg:1", z3.MkBitVecSort(32));
                    var y = z3.MkConst("frame:0:arg:2", z3.MkBitVecSort(32));
                    var z = z3.MkConst("frame:0:arg:3", z3.MkBitVecSort(32));

                    Func<ulong, int, int, int, bool>[] cases = new Func<ulong, int, int, int, bool>[] {
                        (ulong rm, int xv, int yv, int zv) => rm == 1000000000UL && xv + yv == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 1000000000UL && xv + yv != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 2000000000UL && xv - yv == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 2000000000UL && xv - yv != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 3000000000UL && xv * yv == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 3000000000UL && xv * yv != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 4000000000UL && xv / yv == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 4000000000UL && xv / yv != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 5000000000UL && xv % yv == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 5000000000UL && xv % yv != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 6000000000UL && (xv & yv) == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 6000000000UL && (xv & yv) != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 7000000000UL && (xv | yv) == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 7000000000UL && (xv | yv) != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 8000000000UL && (xv ^ yv) == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 8000000000UL && (xv ^ yv) != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 9000000000UL && (xv << yv) == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 9000000000UL && (xv << yv) != zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 10000000000UL && (xv >> yv) == zv,
                        (ulong rm, int xv, int yv, int zv) => rm == 10000000000UL && (xv >> yv) != zv
                    };

                    int caseIndex = 0;
                    foreach (var c in cases)
                    {
                        Assert.IsTrue(helper.ExistsState((s, m) =>
                        {
                            if (TestHelpers.ModelContainsVariables(m, runMode, x, y, z))
                            {
                                var rm = ulong.Parse(m.Evaluate(runMode).ToString());
                                var xv = (int)uint.Parse(m.Evaluate(x).ToString());
                                var yv = (int)uint.Parse(m.Evaluate(y).ToString());
                                var zv = (int)uint.Parse(m.Evaluate(z).ToString());
                                try
                                {
                                    return c.Invoke(rm, xv, yv, zv);
                                } catch (DivideByZeroException)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }), "case " + caseIndex);
                        ++caseIndex;
                    }

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, runMode))
                        {
                            var rm = ulong.Parse(m.Evaluate(runMode).ToString());
                            return rm < 1 || rm > 10;
                        } else
                        {
                            return false;
                        }
                    }));
                }
            }
        }
    }
}
