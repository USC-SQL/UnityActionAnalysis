using System.Collections;

using UnityEngine;
using Unity.AutomatedQA;
using Unity.RecordedPlayback;

namespace UnitySymexCrawler
{

    /**
        * The RecordedPlaybackAutomator does not pass control to the next automator after it finishes playback. 
        * This automator is a wrapper that specifies when to stop the recording playback after a fixed amount of 
        * game time has passed.
        */

    public class RecordedPlaybackTimedAutomatorConfig : AutomatorConfig<RecordedPlaybackTimedAutomator>
    {
        public TextAsset recordingFile = null;
        public bool loadEntryScene = false;
        public float stopTime = 5.0f;
    }

    public class RecordedPlaybackTimedAutomator : Automator<RecordedPlaybackTimedAutomatorConfig>
    {
        private RecordedPlaybackAutomator rpa;

        public override void BeginAutomation()
        {
            base.BeginAutomation();
            rpa.BeginAutomation();
            StartCoroutine(StopAfterWaiting());
        }

        private IEnumerator StopAfterWaiting()
        {
            yield return new WaitForSeconds(config.stopTime);
            RecordedPlaybackController.Instance.Reset();
            EndAutomation();
        }

        public override void Init(RecordedPlaybackTimedAutomatorConfig config)
        {
            base.Init(config);
            rpa = gameObject.AddComponent<RecordedPlaybackAutomator>();
            RecordedPlaybackAutomatorConfig rpaConfig = new RecordedPlaybackAutomatorConfig();
            rpaConfig.recordingFile = config.recordingFile;
            rpaConfig.loadEntryScene = config.loadEntryScene;
            rpa.Init(rpaConfig);
        }
    }

}