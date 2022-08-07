using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityActionAnalysis
{
    public class Symcall
    {
        public readonly MethodInfo method;
        public readonly List<SymexValue> args;

        public Symcall(MethodInfo method, List<SymexValue> args)
        {
            this.method = method;
            this.args = args;
        }
    }
}
