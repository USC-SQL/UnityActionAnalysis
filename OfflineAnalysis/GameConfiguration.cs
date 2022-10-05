using System;
using System.Collections.Generic;
using System.Text;

namespace UnityActionAnalysis
{
    public class OptimizationSettings
    {
        public readonly bool skipNonInputBranches;
        public readonly bool summarizeNonInputMethods;

        public OptimizationSettings(bool skipNonInputBranches = true, bool summarizeNonInputMethods = true)
        {
            this.skipNonInputBranches = skipNonInputBranches;
            this.summarizeNonInputMethods = summarizeNonInputMethods;
        }
    }

    public struct GameConfiguration
    {
        public readonly string assemblyFileName;
        public readonly string outputDatabase;
        public readonly string outputPrecondFuncs;
        public readonly List<string> assemblySearchDirectories;
        public readonly ISet<string> ignoreNamespaces;
        public readonly OptimizationSettings optimizationSettings;

        public GameConfiguration(string assemblyFileName, string outputDatabase, string outputPrecondFuncs, 
            List<string> assemblySearchDirs, ISet<string> ignoreNamespaces, OptimizationSettings optimizationSettings)
        {
            this.assemblyFileName = assemblyFileName;
            this.outputDatabase = outputDatabase;
            this.outputPrecondFuncs = outputPrecondFuncs;
            assemblySearchDirectories = assemblySearchDirs;
            this.ignoreNamespaces = ignoreNamespaces;
            this.optimizationSettings = optimizationSettings;
        }

        public GameConfiguration(string assemblyFileName, string outputDatabase, string outputPrecondFuncs, List<string> assemblySearchDirs, ISet<string> ignoreNamespaces) :
            this(assemblyFileName, outputDatabase, outputPrecondFuncs, assemblySearchDirs, ignoreNamespaces, new OptimizationSettings())
        {
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
