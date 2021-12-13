using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

namespace UnitySymexActionIdentification.Tests
{

    [TestClass()]
    public class TestC
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.TestC.ProgramC", "Main", new TestConfig()))
            {  
                machine.Run();

                TestHelpers.CommonAssertionsAfterRun(machine);

                Assert.AreEqual(3, machine.States.Count);
                Assert.AreEqual(2, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());
                Assert.AreEqual(1, machine.States.Where(s => s.execStatus == ExecutionStatus.ABORTED).Count());

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var arg0 = z3.MkConst("frame:0:arg:0", z3.MkBitVecSort(32));
                    var arg1 = z3.MkConst("frame:0:arg:1", z3.MkBitVecSort(32));
                    var arg2 = z3.MkConst("frame:0:arg:2", z3.MkBitVecSort(64));
                    var arg3 = z3.MkConst("frame:0:arg:3", z3.MkBitVecSort(64));
                    var arg4 = z3.MkConst("frame:0:arg:4", z3.MkBitVecSort(32));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg4))
                        {
                            int length = (int)uint.Parse(m.Evaluate(arg4).ToString());
                            return length < 4;
                        }
                        return true;
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg0, arg1, arg2, arg3, arg4))
                        {
                            int x = (int)uint.Parse(m.Evaluate(arg0).ToString());
                            uint y = uint.Parse(m.Evaluate(arg1).ToString());
                            long z = (long)ulong.Parse(m.Evaluate(arg2).ToString());
                            ulong w = ulong.Parse(m.Evaluate(arg3).ToString());
                            int length = (int)uint.Parse(m.Evaluate(arg4).ToString());

                            ulong arr0 = (uint)x + y;
                            ulong arr1 = (ulong)z + w;
                            ulong arr2 = (uint)(x * y);
                            ulong arr3 = (y + (ulong)z) / w;

                            return length >= 4 && arr0 + arr1 + arr2 + arr3 == 200UL * (uint)length;
                        }
                        return true;
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, arg0, arg1, arg2, arg3, arg4))
                        {
                            int x = (int)uint.Parse(m.Evaluate(arg0).ToString());
                            uint y = uint.Parse(m.Evaluate(arg1).ToString());
                            long z = (long)ulong.Parse(m.Evaluate(arg2).ToString());
                            ulong w = ulong.Parse(m.Evaluate(arg3).ToString());
                            int length = (int)uint.Parse(m.Evaluate(arg4).ToString());

                            ulong arr0 = (uint)x + y;
                            ulong arr1 = (ulong)z + w;
                            ulong arr2 = (uint)(x * y);
                            ulong arr3 = (y + (ulong)z) / w;

                            return length >= 4 && arr0 + arr1 + arr2 + arr3 != 200UL * (uint)length;
                        }
                        return true;
                    }));
                }
            }
        }
    }
}
