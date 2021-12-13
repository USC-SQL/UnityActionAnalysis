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

                Assert.AreEqual(3, machine.States.Count);
                Assert.AreEqual(3, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var rec1id = z3.MkConst("symcall:0:instancefield:id", z3.MkBitVecSort(64));
                    var rec2id = z3.MkConst("symcall:1:instancefield:id", z3.MkBitVecSort(64));
                    var rec1x = z3.MkConst("symcall:0:instancefield:position:instancefield:x", z3.MkFPSortSingle());
                    var rec1y = z3.MkConst("symcall:0:instancefield:position:instancefield:y", z3.MkFPSortSingle());
                    var rec2x = z3.MkConst("symcall:1:instancefield:position:instancefield:x", z3.MkFPSortSingle());
                    var rec2y = z3.MkConst("symcall:1:instancefield:position:instancefield:y", z3.MkFPSortSingle());
                    var rec3x = z3.MkConst("staticfield:TestCases.TestG.GlobalState.thirdRecord:instancefield:position:instancefield:x", z3.MkFPSortSingle());
                    var rec3y = z3.MkConst("staticfield:TestCases.TestG.GlobalState.thirdRecord:instancefield:position:instancefield:y", z3.MkFPSortSingle());

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, rec1id, rec2id, rec1x, rec1y, rec2x, rec2y, rec3x, rec3y))
                        {
                            var r1id = ulong.Parse(m.Evaluate(rec1id).ToString());
                            var r2id = ulong.Parse(m.Evaluate(rec2id).ToString());
                            var r1x = m.Double(rec1x);
                            var r1y = m.Double(rec1y);
                            var r2x = m.Double(rec2x);
                            var r2y = m.Double(rec2y);
                            var r3x = m.Double(rec3x);
                            var r3y = m.Double(rec3y);
                            return r1id < r2id && r1x*r1x + r1y*r1y > r2x*r2x + r2y*r2y + r3x*r3x + r3y*r3y;
                        } else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, rec1id, rec2id, rec1x, rec1y, rec2x, rec2y, rec3x, rec3y))
                        {
                            var r1id = ulong.Parse(m.Evaluate(rec1id).ToString());
                            var r2id = ulong.Parse(m.Evaluate(rec2id).ToString());
                            var r1x = m.Double(rec1x);
                            var r1y = m.Double(rec1y);
                            var r2x = m.Double(rec2x);
                            var r2y = m.Double(rec2y);
                            var r3x = m.Double(rec3x);
                            var r3y = m.Double(rec3y);
                            return r1id < r2id && r1x * r1x + r1y * r1y <= r2x * r2x + r2y * r2y + r3x * r3x + r3y * r3y;
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, rec1id, rec2id))
                        {
                            var r1id = ulong.Parse(m.Evaluate(rec1id).ToString());
                            var r2id = ulong.Parse(m.Evaluate(rec2id).ToString());
                            return r1id >= r2id;
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
