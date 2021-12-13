using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexCrawler
{
    public abstract class Configuration
    {
        public abstract bool IsMethodSymbolic(IMethod method);
    }
}