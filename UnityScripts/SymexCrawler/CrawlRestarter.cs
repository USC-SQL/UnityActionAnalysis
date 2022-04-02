using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AutomatedQA;

namespace UnitySymexCrawler
{
    public class CrawlRestarter : MonoBehaviour
    {
        public string GameObjectNameTrigger = "";
        public string InitialSceneName = "";
        public TextAsset RecordingFile = null;
        public float RecordingDuration = 5.0f;
        public float WaitDuration = 1.0f;

        void Start()
        {
            if (GameObjectNameTrigger.Length == 0)
            {
                throw new Exception("GameObjectNameTrigger not specified");
            }

            if (InitialSceneName.Length == 0)
            {
                throw new Exception("InitialSceneName not specified");
            }

            StartCoroutine("MainLoop");
        }

        private static bool IsEnabled(GameObject trigger)
        {
            if (!trigger.activeInHierarchy)
            {
                return false;
            }

            var canvasGroup = trigger.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                if (!canvasGroup.interactable)
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerator MainLoop()
        {
            for (; ;)
            {
                GameObject target = GameObject.Find(GameObjectNameTrigger);
                if (target != null && IsEnabled(target))
                {
                    var crawler = GetComponent<ICrawler>();
                    if (crawler != null)
                    {
                        crawler.Pause();
                    }
                    SceneManager.LoadScene(InitialSceneName);
                    if (RecordingFile != null)
                    {
                        AutomatedRun.RunConfig runConfig = new AutomatedRun.RunConfig();
                        runConfig.quitOnFinish = false;
                        RecordedPlaybackTimedAutomatorConfig config = new RecordedPlaybackTimedAutomatorConfig();
                        config.recordingFile = RecordingFile;
                        config.loadEntryScene = false;
                        config.stopTime = RecordingDuration;
                        runConfig.automators.Add(config);
                        CentralAutomationController.Instance.Run(runConfig);
                        yield return new WaitForSeconds(RecordingDuration);
                        CentralAutomationController.Instance.Reset();
                    }
                    yield return new WaitForSeconds(WaitDuration);
                    if (crawler != null)
                    {
                        crawler.Resume();
                    }
                }
                yield return null;
            }
        }
    }
}
