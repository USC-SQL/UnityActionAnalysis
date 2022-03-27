using System.Collections;
using UnityEngine;
using Unity.AutomatedQA;

namespace UnitySymexCrawler
{
    public class DelayAutomatorConfig : AutomatorConfig<DelayAutomator>
    {
        public float WaitSeconds;
    }

    public class DelayAutomator : Automator<DelayAutomatorConfig>
    {
        public override void BeginAutomation()
        {
            base.BeginAutomation();
            StartCoroutine(DoWait());
        }

        IEnumerator DoWait()
        {
            yield return new WaitForSeconds(config.WaitSeconds);
            EndAutomation();
            yield break;
        }
    }

}
