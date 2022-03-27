using Unity.AutomatedQA;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class InstantiatePrefabAutomatorConfig : AutomatorConfig<InstantiatePrefabAutomator>
    {
        public GameObject prefab;
    }

    public class InstantiatePrefabAutomator : Automator<InstantiatePrefabAutomatorConfig>
    {
        public override void BeginAutomation()
        {
            base.BeginAutomation();
            Instantiate(config.prefab);
            EndAutomation();
        }
    }

}