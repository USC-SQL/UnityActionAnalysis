using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class SymbolicMethodCallArgument
    {
        public readonly SymexValue value;
        public readonly int argIndex;
        public readonly int symcallId;
        public readonly int pathId;

        public SymbolicMethodCallArgument(SymexValue value, int argIndex, int symcallId, int pathId)
        {
            this.value = value;
            this.argIndex = argIndex;
            this.symcallId = symcallId;
            this.pathId = pathId;
        }
    }
}