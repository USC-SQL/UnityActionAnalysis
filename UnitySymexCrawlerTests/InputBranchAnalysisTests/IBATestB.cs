using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexCrawler.Tests
{
    [TestClass()]
    public class IBATestB
    {
        [TestMethod()]
        public void TestAnalysis()
        {
            InputBranchAnalysis iba = IBATestHelpers.CreateInputBranchAnalysis("TestCases.InputBranchAnalysis.TestB.ProgramB", "Update");
            Dictionary<IMethod, InputBranchAnalysis.MethodAnalysisResult> results = iba.Perform().methodResults;
            
            Assert.IsTrue(results.Count == 1);
            Assert.IsTrue(results.Keys.Any(m => m.Name == "Update"));

            {
                var updateMethod = results.Keys.Where(m => m.Name == "Update").First();
                var updateFunc = IBATestHelpers.FetchMethod(updateMethod, iba.pool);
                var updateResult = results[updateMethod];
                Assert.AreEqual(3, updateResult.inputDepBranchPoints.Count);
                Assert.IsTrue(updateResult.inputDepBranchPoints.Contains(IBATestHelpers.FindInstruction(updateFunc, "switch (ldloc S_14)"))); // switch (amount)
                Assert.IsTrue(updateResult.inputDepBranchPoints.Contains(IBATestHelpers.FindInstruction(updateFunc, "if (comp.i4(ldloc S_23 == ldc.i4 0))"))); // if (q > 5)
                Assert.IsTrue(updateResult.inputDepBranchPoints.Contains(IBATestHelpers.FindInstruction(updateFunc, "if (comp.i4(ldloc S_29 == ldc.i4 0))"))); // if (amount < 50)
                Assert.IsTrue(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_15(ldstr \"A\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_24(ldstr \"B\")")));
                Assert.IsTrue(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_25(ldstr \"C\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_30(ldstr \"D\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_31(ldstr \"E\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_37(ldstr \"F\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_38(ldstr \"G\")")));
                Assert.IsFalse(updateResult.leadsToInputDepBranchPoint.Contains(IBATestHelpers.FindInstruction(updateFunc, "stloc S_44(ldstr \"H\")")));
            }
        }
    }
}
