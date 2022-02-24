using System;
using System.Collections;
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

        public IEnumerator Perform(InputSimulator sim, InputManagerSettings inputManagerSettings, MonoBehaviour context)
        {
            var start = DateTime.Now;
            if (path.SolveForInputs(instance, out ISet<InputCondition> inputConditions))
            {
                foreach (InputCondition cond in contextConditions)
                {
                    inputConditions.Add(cond);
                }

                List<Coroutine> coroutines = new List<Coroutine>();
                foreach (InputCondition cond in inputConditions)
                {
                    coroutines.Add(context.StartCoroutine(cond.PerformInput(sim, inputManagerSettings)));
                }
                foreach (Coroutine coro in coroutines)
                {
                    yield return coro;
                }

                Debug.Log("Performed action in " + (DateTime.Now - start).TotalMilliseconds + "ms: " + string.Join(" && ", inputConditions));
            }
            yield break;
        }
    }
}
