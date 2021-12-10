using ICSharpCode.Decompiler.TypeSystem;

namespace UnitySymexActionIdentification
{
    public abstract class Configuration
    {
        public abstract bool IsMethodSymbolic(IMethod method);
    }
}