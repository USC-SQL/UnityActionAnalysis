using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

namespace UnityActionAnalysis.Tests
{
    [TestClass()]
    public class TestE
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("OfflineAnalysisTestCases.Symex.TestE.ProgramE", "Main", new TestConfig()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(2, machine.States.Count);
                Assert.AreEqual(2, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var arg1 = z3.MkConst("frame:0:arg:1", z3.MkBitVecSort(32));
                    var recordId = z3.MkConst("symcall:0:instancefield:recordId", z3.MkBitVecSort(32));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg1, recordId))
                        {
                            int y = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int rid = (int)uint.Parse(m.Evaluate(recordId).ToString());
                            return y == rid;
                        }
                        else
                        {
                            return false;
                        }
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg1, recordId))
                        {
                            int y = (int)uint.Parse(m.Evaluate(arg1).ToString());
                            int rid = (int)uint.Parse(m.Evaluate(recordId).ToString());
                            return y != rid;
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
