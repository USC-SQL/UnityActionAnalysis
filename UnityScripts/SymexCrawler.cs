using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.Z3;
using Microsoft.Data.Sqlite;

public class SymexCrawler : MonoBehaviour
{
    private SymexDatabase db;

    void Start()
    {
        string dbFile = @"C:\Users\sasha-usc\Misc\UnitySymexCrawler\UnitySymexCrawler\bin\Debug\netcoreapp3.1\symex.db";
        db = new SymexDatabase("Data Source=" + dbFile);
    }

    void Update()
    {
        var gameObjects = FindObjectsOfType(typeof(GameObject));

        List<ISet<InputFact>> actionConditions = new List<ISet<InputFact>>();

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
                            using (var z3 = new Context(new Dictionary<string, string>() {{ "model", "true" }}))
                            {
                                foreach (SymexPath p in db.GetSymexPaths(m))
                                {
                                    if (p.CheckFeasible(component, z3, out var inputFacts))
                                    {
                                        actionConditions.Add(inputFacts);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        Debug.Log(string.Join("\n", actionConditions.Select(cond => string.Join(" && ", cond))));
    }

    void OnDestroy()
    {
        if (db != null)
        {
            db.Dispose();
        }
    }
}
