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

        public GameConfiguration(string assemblyFileName, string outputDatabase, string outputPrecondFuncs, List<string> assemblySearchDirs)
        {
            this.assemblyFileName = assemblyFileName;
            this.outputDatabase = outputDatabase;
            this.outputPrecondFuncs = outputPrecondFuncs;
            assemblySearchDirectories = assemblySearchDirs;
        }
    }

}
