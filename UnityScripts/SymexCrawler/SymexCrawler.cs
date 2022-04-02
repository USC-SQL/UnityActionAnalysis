using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.Z3;
using Microsoft.Data.Sqlite;

using System.IO;
using Newtonsoft.Json;

namespace UnitySymexCrawler
{

    public class SymexCrawler : MonoBehaviour, ICrawler
    {
        public string SymexDatabase;
        public string InputManagerSettings;
        public float Interval = 0.1f;
        public bool Joystick = false;
        public List<string> SkipActionsContaining;
        public string DumpFirstActionSet = "";
        public string DumpRunTimes = ""; 
        public string StatisticsOutputFile = ""; 

        private Dictionary<MethodInfo, SymexMethod> symexMethods;
        private PreconditionFuncs pfuncs;
        private Dictionary<int, MethodInfo> methodsById;
        private Context z3;
        private InputSimulator inputSim;
        private InputManagerSettings inputManagerSettings;
        private StreamWriter runTimeDumpWriter;
        private DateTime createTime;
        private int numActionsPerformed;
        private string runId;
        private bool isPaused;

        private void Start()
        {
            DontDestroyOnLoad(this);

            createTime = DateTime.Now;
            numActionsPerformed = 0;
            var stateDumper = (UnityStateDumper.StateDumper)FindObjectOfType(typeof(UnityStateDumper.StateDumper));
            runId = stateDumper.runId;
            isPaused = false;

            if (SymexDatabase == null || InputManagerSettings == null)
            {
                throw new Exception("Must specify path to Symex Database and InputManager.asset");
            }

            inputManagerSettings = new InputManagerSettings(InputManagerSettings, Joystick ? InputManagerMode.JOYSTICK : InputManagerMode.KEYBOARD);
            
            if (DumpRunTimes.Length > 0)
            {
                runTimeDumpWriter = File.AppendText(DumpRunTimes);
            }

            z3 = new Context(new Dictionary<string, string>() { { "model", "true" } });
            inputSim = Joystick ? (InputSimulator)new JoystickInputSimulator() : new KeyboardInputSimulator();
            inputSim.Reset();

            string dbFile = SymexDatabase;
            using var connection = new SqliteConnection("Data Source=" + dbFile);
            connection.Open();

            methodsById = new Dictionary<int, MethodInfo>();
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "select id, signature from methods";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int methodId = reader.GetInt32(0);
                    string methodSig = reader.GetString(1);
                    MethodInfo method = SymexHelpers.GetMethodFromSignature(methodSig);
                    methodsById.Add(methodId, method);
                }
            }

            symexMethods = new Dictionary<MethodInfo, SymexMethod>();
            pfuncs = new PreconditionFuncs();

            var selectPathsCommand = connection.CreateCommand();
            selectPathsCommand.CommandText = "select id, pathindex, method, condition from paths";
            using var pathsReader = selectPathsCommand.ExecuteReader();
            while (pathsReader.Read())
            {
                int pathId = pathsReader.GetInt32(0);
                int pathIndex = pathsReader.GetInt32(1);
                int methodId = pathsReader.GetInt32(2);
                string pathCondition = pathsReader.GetString(3);
                BoolExpr[] parsedPathCond = z3.ParseSMTLIB2String(pathCondition);

                MethodInfo method = methodsById[methodId];
                SymexMethod m;
                if (!symexMethods.TryGetValue(method, out m))
                {
                    m = new SymexMethod(method);
                    symexMethods.Add(method, m);
                }

                Dictionary<int, Symcall> symcalls = new Dictionary<int, Symcall>();

                var selectSymcallsCommand = connection.CreateCommand();
                selectSymcallsCommand.CommandText = "select symcallid, method from symbolicmethodcalls where pathid = $pathId";
                selectSymcallsCommand.Parameters.AddWithValue("$pathId", pathId);
                using var symcallsReader = selectSymcallsCommand.ExecuteReader();
                while (symcallsReader.Read())
                {
                    int symcallId = symcallsReader.GetInt32(0);
                    int symcallMethodId = symcallsReader.GetInt32(1);
                    MethodInfo symcallMethod = methodsById[symcallMethodId];

                    var selectArgsCommand = connection.CreateCommand();
                    selectArgsCommand.CommandText =
                        "select value from smcarguments where symcallid = $symcallId and pathid = $pathId order by argindex";
                    selectArgsCommand.Parameters.AddWithValue("$symcallId", symcallId);
                    selectArgsCommand.Parameters.AddWithValue("$pathId", pathId);
                    List<SymexValue> args = new List<SymexValue>();
                    using var selectArgsReader = selectArgsCommand.ExecuteReader();
                    while (selectArgsReader.Read())
                    {
                        string value = selectArgsReader.GetString(0);
                        args.Add(SymexValue.Parse(value));
                    }
                    Symcall symcall = new Symcall(symcallMethod, args);
                    symcalls.Add(symcallId, symcall);
                }

                SymexPath p = new SymexPath(pathIndex, parsedPathCond, symcalls, m, z3);
                m.paths.Add(p);
            }

            StartCoroutine("CrawlLoop");
        }

        private List<SymexAction> ComputeAvailableActions()
        {
            var start = DateTime.Now;
            List<SymexAction> actions = new List<SymexAction>();
            int maxNumActions = 0;
            var gameObjects = FindObjectsOfType(typeof(GameObject));
            foreach (var o in gameObjects)
            {
                GameObject gameObject = (GameObject)o;
                if (!gameObject.activeInHierarchy)
                {
                    continue;
                }
                foreach (MonoBehaviour component in gameObject.GetComponents<MonoBehaviour>())
                {
                    Type componentType = component.GetType();
                    foreach (MethodInfo m in componentType.GetMethods(BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.DeclaredOnly))
                    {
                        SymexMethod sm;
                        if (!symexMethods.TryGetValue(m, out sm))
                        {
                            continue;
                        }
                        ISet<InputCondition> contextConditions;
                        if ((m.Name == "Update" || m.Name == "FixedUpdate" || m.Name == "LateUpdate") && m.GetParameters().Length == 0)
                        {
                            contextConditions = new HashSet<InputCondition>();
                        } else
                        {
                            Debug.LogWarning("unexpected method " + m);
                            continue;
                        }

                        foreach (SymexPath p in sm.paths)
                        {
                            if (p.CheckFeasible(component, pfuncs))
                            {
                                actions.Add(new SymexAction(p, component, contextConditions));
                            }
                        }
                        maxNumActions += sm.paths.Count;
                    }
                }
            }
            Debug.Log(actions.Count + "/" + maxNumActions + " actions available (computed in " + (DateTime.Now - start).TotalMilliseconds + "ms)");
            return actions;
        }

        private bool ShouldSkipAction(string inputCondStr)
        {
            foreach (string s in SkipActionsContaining)
            {
                if (inputCondStr.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerator CrawlLoop()
        {
            bool first = true;
            float lastActionTime = Time.realtimeSinceStartup;
            for (; ;)
            {
                if (isPaused || Time.realtimeSinceStartup - lastActionTime < Interval)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                DateTime start = DateTime.Now;
                var actions = ComputeAvailableActions();
                TimeSpan availActionsDuration = DateTime.Now - start;
                if (first && DumpFirstActionSet.Length > 0)
                {
                    ISet<ISet<string>> actionsSer = new HashSet<ISet<string>>();
                    foreach (SymexAction action in actions)
                    {
                        var inputConds = action.TrySolve();
                        if (inputConds != null && inputConds.Count > 0)
                        {
                            ISet<string> actionSer = new HashSet<string>();
                            foreach (InputCondition inputCond in inputConds)
                            {
                                actionSer.Add(inputCond.ToString());
                            }
                            bool alreadyPresent = false;
                            foreach (ISet<string> a in actionsSer)
                            {
                                if (a.SetEquals(actionSer))
                                {
                                    alreadyPresent = true;
                                    break;
                                }
                            }
                            if (!alreadyPresent)
                            {
                                actionsSer.Add(actionSer);
                            }
                        }
                    }
                    if (File.Exists(DumpFirstActionSet))
                    {
                        File.Delete(DumpFirstActionSet);
                    }
                    using (var fs = new StreamWriter(File.OpenWrite(DumpFirstActionSet)))
                    {
                        fs.Write(JsonConvert.SerializeObject(actionsSer, Formatting.Indented));
                    }
                }
                first = false;
                if (actions.Count > 0)
                {
                    DateTime start2 = DateTime.Now;
                    int actionIndex = UnityEngine.Random.Range(0, actions.Count);
                    var selected = actions[actionIndex];
                    var inputConds = selected.TrySolve();
                    if (inputConds != null)
                    {
                        string s = string.Join(" && ", inputConds);
                        if (!ShouldSkipAction(s))
                        {
                            yield return StartCoroutine(inputConds.Perform(inputSim, inputManagerSettings, this));
                            ++numActionsPerformed;
                            lastActionTime = Time.realtimeSinceStartup;
                            TimeSpan actionPerformDuration = DateTime.Now - start2;
                            if (runTimeDumpWriter != null)
                            {
                                runTimeDumpWriter.WriteLine((DateTime.Now - createTime).TotalSeconds + "," + availActionsDuration.TotalMilliseconds + "," + actionPerformDuration.TotalMilliseconds);
                            }
                            Debug.Log("Performed action in " + actionPerformDuration.TotalMilliseconds + "ms: " + s);
                        } else
                        {
                            Debug.Log("Skipped action: " + s);
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private void OnDestroy()
        {
            try
            {
                if (StatisticsOutputFile.Length > 0)
                {
                    using (var sw = new StreamWriter(File.OpenWrite(StatisticsOutputFile + "." + runId + ".json")))
                    {
                        sw.Write(JsonConvert.SerializeObject(new
                        {
                            NumActionsPerformed = numActionsPerformed
                        }));
                    }
                }
            } catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
            }
            if (z3 != null)
            {
                z3.Dispose();
            }
            if (inputSim != null)
            {
                inputSim.Reset();
                inputSim.Dispose();
            }
            if (runTimeDumpWriter != null)
            {
                runTimeDumpWriter.Dispose();
            }
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
        }
    }

}