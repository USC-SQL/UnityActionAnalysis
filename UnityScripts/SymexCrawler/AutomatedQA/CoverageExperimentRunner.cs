using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityStateDumper;
using Unity.AutomatedQA;

namespace UnitySymexCrawler
{
    public class CoverageExperimentRunner : MonoBehaviour
    {
        public List<AutomatedRun> Runs = new List<AutomatedRun>();
        public int Iterations = 5;
        public string InitialScene = "";
        public string DoneNotificationURL = "";
        
        public void Start()
        {
            if (FindObjectsOfType(typeof(CoverageExperimentRunner)).Length > 1)
            {
                Destroy(this);
                return;
            }
            if (InitialScene.Trim().Length == 0)
            {
                throw new Exception("must specify initial scene name");
            }
            DontDestroyOnLoad(this);
            StartCoroutine(RunAutomatedRuns());
        }

        IEnumerator RunAutomatedRuns()
        {
            for (int runIndex = 0; runIndex < Runs.Count; ++runIndex)
            {
                for (int iter = 0; iter < Iterations; ++iter)
                {
                    AutomatedRun run = Runs[runIndex];
                    CentralAutomationController.Instance.Run(run.config);

                    // wait for StateDumper to appear
                    while (FindObjectOfType(typeof(StateDumper)) == null)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    // wait for StateDumper to disappear
                    while (FindObjectOfType(typeof(StateDumper)) != null)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    CentralAutomationController.Instance.Reset();

                    if (iter < Iterations - 1 || runIndex < Runs.Count - 1)
                    {
                        // reset state
                        var symexCrawler = FindObjectOfType(typeof(SymexCrawler));
                        var randomCrawler = FindObjectOfType(typeof(RandomInputCrawler));
                        if (symexCrawler != null)
                        {
                            Destroy(symexCrawler);
                        }
                        if (randomCrawler != null)
                        {
                            Destroy(randomCrawler);
                        }
                        SceneManager.LoadScene(InitialScene);
                    }
                }
            }

            if (DoneNotificationURL.Length > 0) {
                // notify done
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "curl.exe";
                startInfo.Arguments = DoneNotificationURL;
                process.StartInfo = startInfo;
                process.Start();
            }
            EditorApplication.isPlaying = false;
        }
    }
}
