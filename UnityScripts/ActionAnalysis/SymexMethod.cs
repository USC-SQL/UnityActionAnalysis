using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityActionAnalysis
{
    public class SymexMethod
    {
        public MethodInfo method;
        public ISet<SymexPath> paths;

        public SymexMethod(MethodInfo method)
        {
            this.method = method;
            paths = new HashSet<SymexPath>();
        }
    }
}
