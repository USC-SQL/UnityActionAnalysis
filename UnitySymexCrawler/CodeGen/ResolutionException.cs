using System;

namespace UnitySymexCrawler
{
    public class ResolutionException : Exception
    {
        public ResolutionException(string reason) : base(reason)
        {
        }
    }
}
