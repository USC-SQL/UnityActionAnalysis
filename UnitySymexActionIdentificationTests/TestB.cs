using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

namespace UnitySymexActionIdentification.Tests
{
    public class ConfigB : Configuration
    {
        public override bool IsMethodSymbolic(IMethod method)
        {
            return method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule;
        }
    }


    [TestClass()]
    public class TestB
    {

        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.TestB.ProgramB", "Main", new ConfigA()))
            {
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(3, machine.States.Count);
                Assert.AreEqual(2, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());
                Assert.AreEqual(1, machine.States.Where(s => s.execStatus == ExecutionStatus.ABORTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);
                    var arg0 = z3.MkConst("F0_arg0", z3.MkBitVecSort(32));
                    var arg1 = z3.MkConst("F0_arg1", z3.MkBitVecSort(32));
                    var arg2 = z3.MkConst("F0_arg2", z3.MkBitVecSort(32));
                    helper.AssertExistsPathConditionWhere(m =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, "F0_arg0", "F0_arg1", "F0_arg2"))
                        {
                            Console.WriteLine(m.Evaluate(arg0) + ", " + m.Evaluate(arg1) + "," + m.Evaluate(arg2));
                            uint x = uint.Parse(m.Evaluate(arg0).ToString());
                            uint y = uint.Parse(m.Evaluate(arg1).ToString());
                            uint z = uint.Parse(m.Evaluate(arg2).ToString());
                            return (int)(x + y + z) == 10;
                        } else
                        {
                            return false;
                        }
                    });
                    helper.AssertExistsPathConditionWhere(m =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, "F0_arg0", "F0_arg1", "F0_arg2"))
                        {
                            uint x = uint.Parse(m.Evaluate(arg0).ToString());
                            uint y = uint.Parse(m.Evaluate(arg1).ToString());
                            uint z = uint.Parse(m.Evaluate(arg2).ToString());
                            return (int)(x + y + z) == 15;
                        }
                        else
                        {
                            return false;
                        }
                    });
                    helper.AssertExistsPathConditionWhere(m =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, "F0_arg0", "F0_arg1", "F0_arg2"))
                        {
                            uint x = uint.Parse(m.Evaluate(arg0).ToString());
                            uint y = uint.Parse(m.Evaluate(arg1).ToString());
                            uint z = uint.Parse(m.Evaluate(arg2).ToString());
                            int sum = (int)(x + y + z);
                            return sum != 10 && sum != 15;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
            }
        }
    }
}
