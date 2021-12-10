using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

namespace UnitySymexActionIdentification.Tests
{
    public class ConfigA : Configuration
    {
        public override bool IsMethodSymbolic(IMethod method)
        {
            return method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule;
        }
    }

    [TestClass()]
    public class TestA
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.TestA.ProgramA", "Main", new ConfigA()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(3, machine.States.Count);
                Assert.AreEqual(3, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);
                    var arg0 = z3.MkConst("F0_arg0", z3.MkBitVecSort(32));
                    var arg1 = z3.MkConst("F0_arg1", z3.MkBitVecSort(32));
                    helper.AssertExistsPathConditionWhere(m =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, "F0_arg0", "F0_arg1"))
                        {
                            int x = int.Parse(m.Evaluate(arg0).ToString());
                            int y = int.Parse(m.Evaluate(arg1).ToString());
                            return x > 0 && y > 0;
                        } else
                        {
                            return false;
                        }
                    });
                    helper.AssertExistsPathConditionWhere(m =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, "F0_arg0", "F0_arg1"))
                        {
                            int x = int.Parse(m.Evaluate(arg0).ToString());
                            int y = int.Parse(m.Evaluate(arg1).ToString());
                            return x > 0 && y <= 0;
                        } else
                        {
                            return false;
                        }
                    });
                    helper.AssertExistsPathConditionWhere(m =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, "F0_arg0"))
                        {
                            int x = int.Parse(m.Evaluate(arg0).ToString());
                            return x <= 0;
                        } else
                        {
                            return false;
                        }
                    });
                }
            }
        }
    }
}