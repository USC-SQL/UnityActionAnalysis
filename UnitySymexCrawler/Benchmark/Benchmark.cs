using System;
using System.IO;

namespace UnitySymexCrawler
{
    public class Benchmark
    {
        const int Repeat = 10;

        public static void Run()
        {
            foreach (GameConfiguration config in GameConfigs.ALL_CONFIGS)
            {
                for (int i = 0; i < Repeat; ++i)
                {
                    Console.WriteLine("Benchmarking " + config.name + " (iteration " + (i+1) + "/" + Repeat + ")");
                    DateTime start = DateTime.Now;
                    Program.Run(config);
                    var runTimeSec = (DateTime.Now - start).TotalSeconds;
                    using (var sw = File.AppendText("runtime." + config.name + ".log"))
                    {
                        sw.WriteLine(runTimeSec);
                    }
                }
            }
        }

    }
}
