using System;
using System.Collections.Generic;
using System.Text;

namespace UnitySymexCrawler
{
    public struct GameConfiguration
    {
        public readonly string name;
        public readonly string assemblyFileName;
        public readonly string outputDatabase;
        public readonly string outputPrecondFuncs;
        public readonly List<string> assemblySearchDirectories;

        public GameConfiguration(string name, string assemblyFileName, string outputDatabase, string outputPrecondFuncs, List<string> assemblySearchDirs)
        {
            this.name = name;
            this.assemblyFileName = assemblyFileName;
            this.outputDatabase = outputDatabase;
            this.outputPrecondFuncs = outputPrecondFuncs;
            assemblySearchDirectories = assemblySearchDirs;
        }
    }

}
