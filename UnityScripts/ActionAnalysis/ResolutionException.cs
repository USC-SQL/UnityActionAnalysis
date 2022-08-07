using System;

namespace UnityActionAnalysis
{
    public class ResolutionException : Exception
    {
        public ResolutionException(string reason) : base(reason)
        {
        }
    }
}
