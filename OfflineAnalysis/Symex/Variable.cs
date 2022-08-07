using ICSharpCode.Decompiler.TypeSystem;

namespace UnityActionAnalysis
{
    public class Variable
    {
        public readonly IType type;
        public MemoryAddress address;

        public Variable(IType type)
        {
            this.type = type;
            address = null;
        }

        public bool IsReferenceType()
        {
            return type == null;
        }

        public static Variable Reference()
        {
            return new Variable(null);
        }
    }
}
