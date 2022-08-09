using System;
using System.Collections.Generic;
using System.Text;

namespace UnityActionAnalysis
{
    public struct GameConfiguration
    {
        public readonly string assemblyFileName;
        public readonly string outputDatabase;
        public readonly string outputPrecondFuncs;
        public readonly List<string> assemblySearchDirectories;
        public readonly ISet<string> ignoreNamespaces;

        public GameConfiguration(string assemblyFileName, string outputDatabase, string outputPrecondFuncs, List<string> assemblySearchDirs, ISet<string> ignoreNamespaces)
        {
            this.assemblyFileName = assemblyFileName;
            this.outputDatabase = outputDatabase;
            this.outputPrecondFuncs = outputPrecondFuncs;
            assemblySearchDirectories = assemblySearchDirs;
            this.ignoreNamespaces = ignoreNamespaces;
        }

        public bool IsNamespaceIgnored(string ns)
        {
            foreach (string ignNs in ignoreNamespaces)
            {
                if (ns.StartsWith(ignNs))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
