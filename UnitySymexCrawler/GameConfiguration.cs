using System;
using System.Collections.Generic;
using System.Text;

namespace UnitySymexCrawler
{
    public struct GameConfiguration
    {
        public string databaseFileName;
        public string assemblyFileName;
        public List<string> assemblySearchDirectories;

        public GameConfiguration(string databaseFileName, string assemblyFileName, List<string> assemblySearchDirs)
        {
            this.databaseFileName = databaseFileName;
            this.assemblyFileName = assemblyFileName;
            assemblySearchDirectories = assemblySearchDirs;
        }
    }

}
