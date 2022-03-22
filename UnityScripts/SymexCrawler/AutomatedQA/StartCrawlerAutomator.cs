using Unity.AutomatedQA;
using UnityEngine;

public class StartCrawlerAutomatorConfig : AutomatorConfig<StartCrawlerAutomator>
{
    public GameObject crawlerPrefab;
}

public class StartCrawlerAutomator : Automator<StartCrawlerAutomatorConfig>
{
    public override void BeginAutomation()
    {
        base.BeginAutomation();
        Instantiate(config.crawlerPrefab);
        EndAutomation();
    }
}