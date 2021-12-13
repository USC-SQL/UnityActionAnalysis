using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

namespace UnitySymexActionIdentification.Tests
{
    [TestClass()]
    public class TestF
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.TestF.ProgramF", "Main", new TestConfig()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(5, machine.States.Count);
                Assert.AreEqual(5, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var arg_len = z3.MkConst("frame:0:arg:0", z3.MkBitVecSort(32));
                    var arg_xval = z3.MkConst("frame:0:arg:1", z3.MkBitVecSort(32));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg_len, arg_xval))
                        {
                            int len = (int)uint.Parse(m.Evaluate(arg_len).ToString());
                            int xval = (int)uint.Parse(m.Evaluate(arg_xval).ToString());
                            return xval == 20 && len == 10;
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg_len, arg_xval))
                        {
                            int len = (int)uint.Parse(m.Evaluate(arg_len).ToString());
                            int xval = (int)uint.Parse(m.Evaluate(arg_xval).ToString());
                            return xval > 0 && xval != 20 && len == 10;
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg_len, arg_xval))
                        {
                            int len = (int)uint.Parse(m.Evaluate(arg_len).ToString());
                            int xval = (int)uint.Parse(m.Evaluate(arg_xval).ToString());
                            return xval > 0 && len != 10;
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg_len, arg_xval))
                        {
                            int len = (int)uint.Parse(m.Evaluate(arg_len).ToString());
                            int xval = (int)uint.Parse(m.Evaluate(arg_xval).ToString());
                            return xval <= 0 && len == 10;
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg_len, arg_xval))
                        {
                            int len = (int)uint.Parse(m.Evaluate(arg_len).ToString());
                            int xval = (int)uint.Parse(m.Evaluate(arg_xval).ToString());
                            return xval <= 0 && len != 10;
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
