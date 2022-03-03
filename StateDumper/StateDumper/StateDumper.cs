using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;
using System.IO;
using System.Threading;

public class StateDumper : MonoBehaviour
{
    public string DumpDir = "";

    private static float lastDumpTime = 0.0f;

    public struct Entry
    {
        public string outFile;
        public object state;
        public bool stop;
    }

    public static Thread stateDumpThread = null;
    public static Queue<Entry> stateDumpQueue = new Queue<Entry>();
    public static bool printDumpsRemaining = false;

    private static void Log(string msg)
    {
        Debug.Log(msg);
    }

    private static bool ShouldStop(string dumpDir)
    {
        return File.Exists(dumpDir + "/stop");
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

    private void Start()
    {
        if (DumpDir == null)
        {
            DumpDir = Application.persistentDataPath + "/dump-" + ((int)((DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000L)).ToString();
            Directory.CreateDirectory(DumpDir);
            Log("Dumping state to: " + DumpDir);
        }

        if (stateDumpThread == null)
        {
            stateDumpThread = new Thread(ThreadProc);
            stateDumpThread.Start();
        }
    }

    private bool notifiedStop = false;

    private void ResetSerializeState()
    {
    }

    public object SerializeGameObject(GameObject gameObject)
    {
        var cs = gameObject.GetComponents<Component>();

        List<object> children = new List<object>(gameObject.transform.childCount);

        var childCount = gameObject.transform.childCount;
        for (int i = 0; i != childCount; ++i)
        {
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            if (child.activeInHierarchy)
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
            Scene scn = root.scene;
            HashSet<GameObject> roots;
            if (!rootGameObjects.TryGetValue(scn, out roots))
            {
                roots = new HashSet<GameObject>();
                rootGameObjects.Add(scn, roots);
            }
            roots.Add(root);
        }

        List<SceneInfo> scenes = new List<SceneInfo>();
        foreach (KeyValuePair<Scene, HashSet<GameObject>> p in rootGameObjects)
        {
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

        if (Time.time - lastDumpTime >= dumpInterval)
        {
            object state = SerializeGame();
            string outFile = DumpDir + "/state-" + Time.time + ".json";

            Entry e = new Entry();
            e.state = state;
            e.outFile = outFile;
            lock (stateDumpQueue)
            {
                stateDumpQueue.Enqueue(e);
            }

            lastDumpTime = Time.time;
        }
    }
}
