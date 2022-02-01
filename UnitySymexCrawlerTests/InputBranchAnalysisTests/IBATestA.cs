using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexCrawler.Tests
{
    [TestClass()]
    public class IBATestA
    {
        [TestMethod()]
        public void TestAnalysis()
        {
            InputBranchAnalysis iba = IBATestHelpers.CreateInputBranchAnalysis("TestCases.InputBranchAnalysis.TestA.ProgramA", "Update");
            Dictionary<IMethod, InputBranchAnalysis.MethodAnalysisResult> results = iba.Perform();

            Assert.IsTrue(results.Count == 4);
            Assert.IsTrue(results.Keys.Any(m => m.Name == "Update"));
            Assert.IsTrue(results.Keys.Any(m => m.Name == "f"));
            Assert.IsTrue(results.Keys.Any(m => m.Name == "AxisInverted"));
            Assert.IsTrue(results.Keys.Any(m => m.Name == "Sum"));

            {
                var sumResult = results[results.Keys.Where(m => m.Name == "Sum").First()];
                Assert.AreEqual(0, sumResult.inputDepBranchPoints.Count);
                Assert.AreEqual(0, sumResult.leadsToInputDepBranchPoint.Count);
            }

            {
                var axisInvertedResult = results[results.Keys.Where(m => m.Name == "AxisInverted").First()];
                Assert.AreEqual(0, axisInvertedResult.inputDepBranchPoints.Count);
                Assert.AreEqual(0, axisInvertedResult.leadsToInputDepBranchPoint.Count);
            }

            {
                var fMethod = results.Keys.Where(m => m.Name == "f").First();
                var fFunc = IBATestHelpers.FetchMethod(fMethod, iba.pool);
                var fResult = results[fMethod];
                Assert.AreEqual(2, fResult.inputDepBranchPoints.Count);
                Assert.IsTrue(fResult.inputDepBranchPoints.Contains(IBATestHelpers.FindInstruction(fFunc, "if (comp.i4(ldloc S_8 == ldc.i4 0))"))); // if (y > 0.0f)
                Assert.IsTrue(fResult.inputDepBranchPoints.Contains(IBATestHelpers.FindInstruction(fFunc, "if (comp.i4(ldloc S_12 == ldc.i4 0))"))); // if (Input.GetButtonDown("Fire"))
                Assert.IsTrue(fResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.Entrypoint(fFunc)));
                Assert.IsTrue(fResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(fFunc, "stloc S_9(ldstr \"A\")")));
                Assert.IsFalse(fResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(fFunc, "stloc S_13(ldstr \"B\")")));
                Assert.IsFalse(fResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(fFunc, "stloc S_14(ldstr \"C\")")));
            }

            {
                var updateMethod = results.Keys.Where(m => m.Name == "Update").First();
                var updateFunc = IBATestHelpers.FetchMethod(updateMethod, iba.pool);
                var updateResult = results[updateMethod];
                Assert.AreEqual(2, updateResult.inputDepBranchPoints.Count);
                Assert.IsTrue(updateResult.inputDepBranchPoints.Contains(IBATestHelpers.FindInstruction(updateFunc, "if (comp.i4(ldloc S_6 == ldc.i4 0))"))); // if (b2)
                Assert.IsTrue(updateResult.inputDepBranchPoints.Contains(IBATestHelpers.FindInstruction(updateFunc, "if (comp.i4(ldloc S_15 == ldc.i4 0))"))); // if (vert > 0.0f)
                Assert.IsTrue(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.Entrypoint(updateFunc)));
                Assert.IsTrue(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_7(ldstr \"A\")")));
                Assert.IsTrue(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_8(ldstr \"B\")")));
                Assert.IsTrue(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_16(ldstr \"C\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_18(ldstr \"D\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_29(ldstr \"E\")")));
            }
        }
    }
}
