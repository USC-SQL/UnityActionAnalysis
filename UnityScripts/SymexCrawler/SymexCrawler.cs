using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.Z3;
using Microsoft.Data.Sqlite;

namespace UnitySymexCrawler
{

    public class SymexCrawler : MonoBehaviour
    {
        public string SymexDatabase;
        public float Interval = 0.1f;

        private Dictionary<MethodInfo, SymexMethod> symexMethods;
        private PreconditionFuncs pfuncs;
        private Dictionary<int, MethodInfo> methodsById;
        private Context z3;
        private InputSimulator inputSim;

        private void Start()
        {
            z3 = new Context(new Dictionary<string, string>() { { "model", "true" } });
            inputSim = new InputSimulator();

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

        public IEnumerator CrawlLoop()
        {
            yield return new WaitForSeconds(Interval);
            for (; ;)
            {
                var actions = ComputeAvailableActions();
                if (actions.Count > 0)
                {
                    int actionIndex = UnityEngine.Random.Range(0, actions.Count);
                    var selected = actions[actionIndex];
                    yield return StartCoroutine(selected.Perform(inputSim, this));
                }
                yield return new WaitForSeconds(Interval);
            }
        }

        private void OnDestroy()
        {
            if (z3 != null)
            {
                z3.Dispose();
            }
        }
    }

}