using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class SymexAction
    {
        private readonly SymexPath path;
        private readonly MonoBehaviour instance;
        private readonly ISet<InputCondition> contextConditions;

        public SymexAction(SymexPath path, MonoBehaviour instance, ISet<InputCondition> contextConditions)
        {
            this.path = path;
            this.instance = instance;
            this.contextConditions = contextConditions;
        }

        public void Perform(InputSimulator sim)
        {
            var start = DateTime.Now;
            if (path.SolveForInputs(instance, out ISet<InputCondition> inputConditions))
            {
                foreach (InputCondition cond in contextConditions)
                {
                    inputConditions.Add(cond);
                }

                foreach (InputCondition cond in inputConditions)
                {
                    cond.PerformInput(sim);
                }

                Debug.Log("Performed action in " + (DateTime.Now - start).TotalMilliseconds + "ms: " + string.Join(" && ", inputConditions));
            }
        }
    }
}
