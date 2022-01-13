using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;
using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexCrawler.Tests
{
    public class ConfigK : TestConfig
    {
    }

    [TestClass()]
    public class TestK
    {
        [TestMethod()]
        public void TestPathConditions()
        {
            using (SymexMachine machine = TestHelpers.CreateMachine("TestCases.TestK.ProgramK", "Main", new ConfigK()))
            {
                machine.Run();

                Assert.AreEqual(1, machine.States.Count);
            }
        }
    }
}
