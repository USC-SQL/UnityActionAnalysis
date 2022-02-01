using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexCrawler.Tests
{
    public class TestConfig : Configuration
    {
        public override bool IsMethodSymbolic(IMethod method)
        {
            return method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule;
        }
    }
}
