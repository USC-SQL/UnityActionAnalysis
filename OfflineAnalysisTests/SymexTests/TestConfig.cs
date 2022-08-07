using ICSharpCode.Decompiler.TypeSystem;

namespace UnityActionAnalysis.Tests
{
    public class TestConfig : Configuration
    {
        public override bool IsMethodSummarized(IMethod method)
        {
            return method.ParentModule != SymexMachine.Instance.CSD.TypeSystem.MainModule;
        }
    }
}
