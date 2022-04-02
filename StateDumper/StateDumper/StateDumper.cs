using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.TestTools.CodeCoverage;
using Newtonsoft.Json;

namespace UnityStateDumper
{
    public class StateDumper : MonoBehaviour
    {
        public string DumpDir = "";
        public float Duration = float.PositiveInfinity; // seconds
        public string RecordCodeCoverage = ""; // if empty, not recorded, otherwise written to specified folder after duration has passed
        public string MoveCodeCoverageFrom = "";
        public string WriteDummyStats = "";
        public float CodeCoverageSampleRate = 2.0f;
        public bool StopGameAfterDuration = false;

        private float startTime;
        public string runId { get; private set; }
        private float lastDumpTime;
        private float lastCodeCovSampleTime;

        public struct Entry
        {
            public string outFile;
            public object state;
            public bool stop;
        }

        public static Thread stateDumpThread = null;
        public static Queue<Entry> stateDumpQueue = new Queue<Entry>();
        public static bool printDumpsRemaining = false;
        private static bool? prevBatchmode;
        private static bool? prevUseProjectSettings;
        
        private static void Log(string msg)
        {
            Debug.Log(msg);
        }

        private static bool ShouldStop(string dumpDir)
        {
            return File.Exists(dumpDir + "/stop");
        }

        private static bool IgnoreGameObject(GameObject go)
        {
            return go.name.StartsWith("Unity.RecordedPlayback") || go.name.Equals("StartRecordedPlaybackFromEditor");
        }

        public static void ThreadProc()
        {
            for (; ; )
            {
                lock (stateDumpQueue)
                {
                    if (stateDumpQueue.Count > 0)
                    {
                        var entry = stateDumpQueue.Dequeue();

                        if (entry.stop)
                        {
                            Log("State dumping done!");
                            break;
                        }

                        using (FileStream fs = File.Create(entry.outFile))
                        {
                            string json = Tiny.Json.Encode(entry.state, false);
                            byte[] bytes = new UTF8Encoding(true).GetBytes(json);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        // hack so the code coverage GUI doesn't pop up whenever we sample the code coverage
        private static void ForceCodeCovBatchmode(bool setting)
        {
            Type cmdLineManager = Type.GetType("UnityEditor.TestTools.CodeCoverage.CommandLineManager,Unity.TestTools.CodeCoverage.Editor");
            Type cmdLineManagerImpl = Type.GetType("UnityEditor.TestTools.CodeCoverage.CommandLineManagerImplementation,Unity.TestTools.CodeCoverage.Editor");
            object cmdLineManagerInst = cmdLineManager.GetMethod("get_instance").Invoke(null, new object[0]);
            var batchmode = cmdLineManagerImpl.GetProperty("batchmode");
            prevBatchmode = (bool)batchmode.GetValue(cmdLineManagerInst);
            batchmode.SetValue(cmdLineManagerInst, setting);
        }

        private static void ForceCodeCovUseProjectSettings(bool setting)
        {
            Type cmdLineManager = Type.GetType("UnityEditor.TestTools.CodeCoverage.CommandLineManager,Unity.TestTools.CodeCoverage.Editor");
            Type cmdLineManagerImpl = Type.GetType("UnityEditor.TestTools.CodeCoverage.CommandLineManagerImplementation,Unity.TestTools.CodeCoverage.Editor");
            object cmdLineManagerInst = cmdLineManager.GetMethod("get_instance").Invoke(null, new object[0]);
            var useProjectSettings = cmdLineManagerImpl.GetProperty("useProjectSettings");
            prevUseProjectSettings = (bool)useProjectSettings.GetValue(cmdLineManagerInst);
            useProjectSettings.SetValue(cmdLineManagerInst, setting);
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
            prevBatchmode = null;

            if (DumpDir == null)
            {
                DumpDir = Application.persistentDataPath + "/dump-" + ((int)((DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000L)).ToString();
                Directory.CreateDirectory(DumpDir);
                Log("Dumping state to: " + DumpDir);
            }

            if (!Directory.Exists(DumpDir))
            {
                Directory.CreateDirectory(DumpDir);
            }
            runId = ((DateTime.Now.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000L).ToString();
            Directory.CreateDirectory(Path.Combine(DumpDir, runId));

            if (stateDumpThread == null)
            {
                stateDumpThread = new Thread(ThreadProc);
                stateDumpThread.Start();
            }

            startTime = Time.realtimeSinceStartup;
            lastDumpTime = startTime;
            lastCodeCovSampleTime = startTime;

            if (RecordCodeCoverage.Length > 0)
            {
                ForceCodeCovBatchmode(true);
                ForceCodeCovUseProjectSettings(true);

                CodeCoverage.StopRecording(); // stop any existing recording
                if (!Directory.Exists(MoveCodeCoverageFrom))
                {
                    Directory.CreateDirectory(MoveCodeCoverageFrom);
                }
                if (Directory.GetFiles(MoveCodeCoverageFrom).Length > 0)
                {
                    throw new Exception("folder to move code coverage from already has code coverage data");
                }
                if (!Directory.Exists(RecordCodeCoverage))
                {
                    Directory.CreateDirectory(RecordCodeCoverage);
                }
                Directory.CreateDirectory(Path.Combine(RecordCodeCoverage, runId));
                CodeCoverage.StartRecording();
            }
        }

        private bool notifiedStop = false;

        private void ResetSerializeState()
        {
        }

        public object SerializeGameObject(GameObject gameObject)
        {
            List<object> children = new List<object>(gameObject.transform.childCount);

            var childCount = gameObject.transform.childCount;
            for (int i = 0; i != childCount; ++i)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                if (child.activeInHierarchy && !IgnoreGameObject(child))
                {
                    children.Add(SerializeGameObject(child));
                }
            }

            return new
            {
                gameObject = new
                {
                    gameObject.layer,
                    gameObject.name,
                    gameObject.tag
                },
                components = new Dictionary<string, object>(),
                children
            };
        }

        struct SceneInfo
        {
            public Scene scn;
            public List<GameObject> roots;
        }

        public object SerializeGame()
        {
            ResetSerializeState();

            UnityEngine.Object[] allGameObjects = FindObjectsOfType(typeof(GameObject));
            Dictionary<Scene, HashSet<GameObject>> rootGameObjects = new Dictionary<Scene, HashSet<GameObject>>();

            foreach (UnityEngine.Object go in allGameObjects)
            {
                var root = ((GameObject)go).transform.root.gameObject;
                if (root.activeInHierarchy && !IgnoreGameObject(root))
                {
                    Scene scn = root.scene;
                    HashSet<GameObject> roots;
                    if (!rootGameObjects.TryGetValue(scn, out roots))
                    {
                        roots = new HashSet<GameObject>();
                        rootGameObjects.Add(scn, roots);
                    }
                    roots.Add(root);
                }
            }

            List<SceneInfo> scenes = new List<SceneInfo>();
            foreach (KeyValuePair<Scene, HashSet<GameObject>> p in rootGameObjects)
            {
                if (p.Key.name == "DontDestroyOnLoad")
                {
                    continue;
                }
                SceneInfo info = new SceneInfo()
                {
                    scn = p.Key,
                    roots = new List<GameObject>(p.Value)
                };
                info.roots.Sort((a, b) => a.transform.GetSiblingIndex() - b.transform.GetSiblingIndex());
                scenes.Add(info);
            }

            scenes.Sort((a, b) => a.scn.buildIndex - b.scn.buildIndex);

            List<object> result = new List<object>();
            foreach (SceneInfo info in scenes)
            {
                List<object> rgos = new List<object>(info.roots.Count);
                foreach (GameObject rgo in info.roots)
                {
                    if (rgo.activeInHierarchy)
                    {
                        rgos.Add(SerializeGameObject(rgo));
                    }
                }
                result.Add(new
                {
                    name = info.scn.name,
                    rootGameObjects = rgos
                });
            }

            return new
            {
                scenes = result
            };
        }

        private void Update()
        {
            float dumpInterval = 0.5f;

            if (ShouldStop(DumpDir))
            {
                if (!notifiedStop)
                {
                    Entry e = new Entry();
                    e.stop = true;
                    lock (stateDumpQueue)
                    {
                        stateDumpQueue.Enqueue(e);
                    }
                    printDumpsRemaining = true;
                    notifiedStop = true;
                }
                return;
            }

            if (Time.realtimeSinceStartup - lastDumpTime >= dumpInterval)
            {
                object state = SerializeGame();
                string outFile = Path.Combine(DumpDir, runId, "state-" + (Time.realtimeSinceStartup - startTime) + ".json");

                Entry e = new Entry();
                e.state = state;
                e.outFile = outFile;
                lock (stateDumpQueue)
                {
                    stateDumpQueue.Enqueue(e);
                }

                lastDumpTime = Time.realtimeSinceStartup;
            }

            if (Time.realtimeSinceStartup - lastCodeCovSampleTime >= CodeCoverageSampleRate)
            {
                CodeCoverage.PauseRecording();
                CodeCoverage.UnpauseRecording();
                lastCodeCovSampleTime = Time.realtimeSinceStartup;
            }

            if (Time.realtimeSinceStartup - startTime >= Duration)
            {
                if (RecordCodeCoverage.Length > 0)
                {
                    RevertForcedCodeCovSettings();
                    CodeCoverage.StopRecording();

                    foreach (var file in Directory.GetFileSystemEntries(MoveCodeCoverageFrom))
                    {
                        File.Move(file, Path.Combine(RecordCodeCoverage, runId, Path.GetFileName(file)));
                    }
                    UnityEngine.TestTools.Coverage.ResetAll();
                }
                if (WriteDummyStats.Length > 0)
                {
                    using (var sw = new StreamWriter(File.OpenWrite(WriteDummyStats + "." + runId + ".json")))
                    {
                        sw.Write(JsonConvert.SerializeObject(new { }));
                    }
                }
                if (StopGameAfterDuration)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                }
                Destroy(this);
            }
        }

        private void RevertForcedCodeCovSettings()
        {
            if (prevBatchmode.HasValue)
            {
                ForceCodeCovBatchmode(prevBatchmode.Value);
                ForceCodeCovUseProjectSettings(prevUseProjectSettings.Value);
                prevBatchmode = null;
                prevUseProjectSettings = null;
            }
        }

        void OnDestroy()
        {
            RevertForcedCodeCovSettings();
        }
    }
}
