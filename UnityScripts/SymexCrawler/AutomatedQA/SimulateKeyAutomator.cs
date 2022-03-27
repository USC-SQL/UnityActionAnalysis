using System.Collections;
using UnityEngine;
using Unity.AutomatedQA;

namespace UnitySymexCrawler
{
    public class SimulateKeyAutomatorConfig : AutomatorConfig<SimulateKeyAutomator>
    {
        public KeyCode keyCode;
    }

    public class SimulateKeyAutomator : Automator<SimulateKeyAutomatorConfig>
    {
        private InputSimulator inputSim;

        public override void BeginAutomation()
        {
            base.BeginAutomation();
            inputSim = new KeyboardInputSimulator();
            StartCoroutine(DoSimulate());
        }

        IEnumerator DoSimulate()
        {
            yield return StartCoroutine(new KeyDownInputCondition(config.keyCode, true).PerformInput(inputSim, null));
            EndAutomation();
        }

        public void OnDestroy()
        {
            inputSim.Dispose();
        }
    }
}
