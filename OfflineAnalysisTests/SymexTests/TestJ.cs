using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnityActionAnalysis.Tests
{
    public class ConfigJ : TestConfig
    {
        public override bool IsMethodSummarized(IMethod method)
        {
            return base.IsMethodSummarized(method) || method.Name == "GetAxis";
        }

        public override void ApplyMethodSummary(IMethod method, List<Expr> arguments, Variable resultVar, SymexState state)
        {
            int symId = -1;
            if (method.Name == "GetAxis")
            {
                string arg = JsonSerializer.Serialize(state.SerializeExpr(arguments[0]));
                foreach (var p in state.symbolicMethodCalls)
                {
                    SymbolicMethodCall smc = p.Value;
                    if (smc.method.Name == "GetAxis" && JsonSerializer.Serialize(state.SerializeExpr(smc.args[0])) == arg)
                    {
                        symId = p.Key;
                        break;
                    }
                }
            }
            if (symId < 0)
            {
                symId = state.symcallCounter++;
            }
            ApplySymcallMethodSummary(symId, method, arguments, resultVar, state);
        }
    }

    [TestClass()]
    public class TestJ
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("OfflineAnalysisTestCases.Symex.TestJ.ProgramJ", "Main", new ConfigJ()))
            {
                machine.Run();

                Assert.AreEqual(7, machine.States.Count);
                Assert.AreEqual(7, machine.States.Where(s => s.execStatus == ExecutionStatus.HALTED).Count());

                TestHelpers.CommonAssertionsAfterRun(machine);

                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                {
                    TestHelpers.SymexMachineHelper helper = new TestHelpers.SymexMachineHelper(machine, z3);

                    var symcall0 = z3.MkConst("symcall:0", z3.MkBitVecSort(32));
                    var symcall1 = z3.MkConst("symcall:1", z3.MkBitVecSort(32));

                    Assert.IsTrue(helper.ForAllStates((s, m) =>
                    {
                        return m.ConstDecls.Length >= 1 && m.ConstDecls.Length <= 2;
                    }));

                    Assert.IsTrue(helper.ExistsState((s, m) =>
                    {
                        if (TestHelpers.ModelContainsVariables(m, symcall0))
                        {
                            int h = (int)uint.Parse(m.Evaluate(symcall0).ToString());
                            return h == 0;
                        } else
                        {
                            return false;
                        }
                    }));

                    Func<int, int, bool>[] cases2 = new Func<int, int, bool>[]
                    {
                        (int h, int v) => h > 0 && v > 0,
                        (int h, int v) => h > 0 && v < 0,
                        (int h, int v) => h > 0 && v == 0,
                        (int h, int v) => h < 0 && v > 0,
                        (int h, int v) => h < 0 && v < 0,
                        (int h, int v) => h < 0 && v == 0
                    };

                    int index = 0;
                    foreach (var c in cases2)
                    {
                        Assert.IsTrue(helper.ExistsState((s, m) =>
                        {
                            if (TestHelpers.ModelContainsVariables(m, symcall0, symcall1))
                            {
                                string axis0 = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(s.SerializeExpr(s.symbolicMethodCalls[0].args[0]))).GetProperty("value").GetString();
                                Expr symcallHorizontal, symcallVertical;
                                if (axis0 == "Horizontal")
                                {
                                    symcallHorizontal = symcall0;
                                    symcallVertical = symcall1;
                                }
                                else
                                {
                                    symcallHorizontal = symcall1;
                                    symcallVertical = symcall0;
                                }

                                int h = (int)uint.Parse(m.Evaluate(symcallHorizontal).ToString());
                                int v = (int)uint.Parse(m.Evaluate(symcallVertical).ToString());
                                return c(h, v);
                            }
                            else
                            {
                                return false;
                            }
                        }), "cases2[" + index + "]");
                        ++index;
                    }
                }
            }
        }
    }
}
