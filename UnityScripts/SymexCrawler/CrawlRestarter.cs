using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AutomatedQA;

namespace UnitySymexCrawler
{
    public class CrawlRestarter : MonoBehaviour
    {
#if !UNITY_AWESOME_RUNNER
        public string GameObjectNameTrigger = "";
#endif
        public string InitialSceneName = "";
        public TextAsset RecordingFile = null;
        public float RecordingDuration = 5.0f;
        public float WaitDuration = 1.0f;

        void Start()
        {
            DontDestroyOnLoad(this);

            #if !UNITY_AWESOME_RUNNER
            if (GameObjectNameTrigger.Length == 0)
            {
                throw new Exception("GameObjectNameTrigger not specified");
            }
            #endif

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
            if (canvasGroup != null && (!canvasGroup.isActiveAndEnabled || !canvasGroup.interactable))
            {
                return false;
            }

            var canvas = trigger.GetComponent<Canvas>();
            if (canvas != null && !canvas.isActiveAndEnabled)
            {
                return false;
            }

            return true;
        }

        IEnumerator MainLoop()
        {
            for (; ;)
            {
                bool shouldRestart;
#if UNITY_AWESOME_RUNNER
                shouldRestart = GameManager.instance.gameRestartedPlayerDied && GameManager.instance.health < 0;
#else
                GameObject target = GameObject.Find(GameObjectNameTrigger);
                shouldRestart = target != null && IsEnabled(target);
#endif
                if (shouldRestart)
                {
                    var crawler = GetComponent<ICrawler>();
                    if (crawler != null)
                    {
                        crawler.Pause();
                    }
                    SceneManager.LoadScene(InitialSceneName);
                    Time.timeScale = 1.0f;
#if PACMAN_CLONE
                    GameManager.DestroySelf();
#elif UNITY_AWESOME_RUNNER
                    GameManager.instance.gameRestartedPlayerDied = false;
                    GameManager.instance.health = 3;
#endif
                    if (RecordingFile != null)
                    {
                        AutomatedRun.RunConfig runConfig = new AutomatedRun.RunConfig();
                        runConfig.quitOnFinish = false;
                        RecordedPlaybackTimedAutomatorConfig config = new RecordedPlaybackTimedAutomatorConfig();
                        config.recordingFile = RecordingFile;
                        config.loadEntryScene = false;
                        config.stopTime = RecordingDuration;
                        runConfig.automators.Add(config);
                        CentralAutomationController.Instance.Reset();
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
