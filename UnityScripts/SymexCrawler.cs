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
        private SymexDatabase db;
        private InputSimulator sim;

        public string SymexDatabase;

        void Start()
        {
            string dbFile = SymexDatabase;
            db = new SymexDatabase("Data Source=" + dbFile);
            sim = new InputSimulator();
            StartCoroutine("CrawlLoop");
        }

        private List<Action> ComputeActions()
        {
            var gameObjects = FindObjectsOfType(typeof(GameObject));

            List<Action> actions = new List<Action>();
            
            SymexPath.ResetTimers();

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
                        if ((m.Name == "Update" || m.Name == "FixedUpdate" || m.Name == "LateUpdate") && m.GetParameters().Length == 0)
                        {
                            if (db.IsSymexMethod(m))
                            {
                                using (var z3 = new Context(new Dictionary<string, string>() { { "model", "true" } }))
                                {
                                    var paths = db.GetSymexPaths(m);
                                    Debug.Log(paths.Count + " paths");
                                    foreach (SymexPath p in paths)
                                    {
                                        if (p.CheckSatisfiable(component, z3, out var pathCondition))
                                        {
                                            actions.Add(new Action(pathCondition));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            SymexPath.PrintTimers();

            return actions;
        }

        public IEnumerator CrawlLoop()
        {
            yield return new WaitForSeconds(0.25f);
            for (; ; )
            {
                var start = DateTime.Now;
                var actions = ComputeActions();
                var possibleActions = actions.Where(action => action.CanPerform()).ToList();
                Debug.Log("possible: [" + string.Join(", ", possibleActions) + "]");
                if (possibleActions.Count > 0)
                {
                    int actionIndex = (int)UnityEngine.Random.Range(0.0f, possibleActions.Count - 0.001f);
                    var selected = possibleActions[actionIndex];
                    Debug.Log("selected: " + selected);
                    selected.Perform(sim);
                }
                var end = DateTime.Now;
                Debug.Log("crawl iteration took " + (end - start).TotalMilliseconds + "ms");
                yield return new WaitForSeconds(0.25f);
            }
        }

        void OnDestroy()
        {
            if (db != null)
            {
                db.Dispose();
            }
        }
    }
}