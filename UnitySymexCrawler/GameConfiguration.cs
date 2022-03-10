using System;
using System.Collections.Generic;
using System.Text;

namespace UnitySymexCrawler
{
    public struct GameConfiguration
    {
        public string assemblyFileName;
        public string outputDatabase;
        public string outputPrecondFuncs;
        public List<string> assemblySearchDirectories;

        public GameConfiguration(string assemblyFileName, string outputDatabase, string outputPrecondFuncs, List<string> assemblySearchDirs)
        {
            this.assemblyFileName = assemblyFileName;
            this.outputDatabase = outputDatabase;
            this.outputPrecondFuncs = outputPrecondFuncs;
            assemblySearchDirectories = assemblySearchDirs;
        }
    }

}
