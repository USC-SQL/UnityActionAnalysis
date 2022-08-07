﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using CommandLine;

namespace UnityActionAnalysis
{
    public class Program
    {

        public class Options
        {
            [Option("instrument", 
                Required = false, 
                Default = false,
                HelpText = "Instead of analyzing the assembly, instrument the assembly's invocations to Input APIs to be compatible with InstrInputSimulator")]
            public bool Instrument { get; set; }

            [Value(0, 
                MetaName = "config", 
                HelpText = "Path to JSON file describing the input assembly and analysis output paths, see README for configuration format")]
            public string ConfigJsonPath { get; set;  }
        }

        private static GameConfiguration ParseConfiguration(JsonElement configJson)
        {
            string configAssemblyFileName;
            if (configJson.TryGetProperty("assemblyPath", out JsonElement assemblyPath))
            {
                configAssemblyFileName = assemblyPath.GetString();
            } else
            {
                throw new Exception("config is missing 'assemblyPath'");
            }

            string configOutputDatabase = null;
            if (configJson.TryGetProperty("databaseOutputDirectory", out JsonElement databaseOutputDirectory))
            {
                configOutputDatabase = Path.Combine(databaseOutputDirectory.GetString(), "paths.db");
            }

            string configOutputPrecondFuncs = null;
            if (configJson.TryGetProperty("scriptOutputDirectory", out JsonElement scriptOutputDirectory))
            {
                configOutputPrecondFuncs = Path.Combine(scriptOutputDirectory.GetString(), "PreconditionFuncs.cs");
            }

            List<string> configAssemblySearchDirectories;
            if (configJson.TryGetProperty("assemblySearchDirectories", out JsonElement assemblySearchDirectories))
            {
                if (assemblySearchDirectories.ValueKind != JsonValueKind.Array)
                {
                    throw new Exception("config assemblySearchDirectories should be an array");
                }
                int numElems = assemblySearchDirectories.GetArrayLength();
                configAssemblySearchDirectories = new List<string>(numElems);
                for (int i = 0; i < numElems; ++i)
                {
                    JsonElement elem = assemblySearchDirectories[i];
                    configAssemblySearchDirectories.Add(elem.GetString());
                }
            } else
            {
                configAssemblySearchDirectories = new List<string>();
            }

            return new GameConfiguration(configAssemblyFileName, configOutputDatabase, configOutputPrecondFuncs, configAssemblySearchDirectories);
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        static void Run(Options opts)
        {
            JsonElement configJson;
            using (var fs = File.OpenRead(opts.ConfigJsonPath))
            {
                configJson = JsonSerializer.Deserialize<JsonElement>(fs);
            }

            GameConfiguration config = ParseConfiguration(configJson);

            if (opts.Instrument)
            {
                InputInstrumentation.Run(config);
            } else
            {
                OfflineAnalysis.Run(config);
            }
        }
    }
}
