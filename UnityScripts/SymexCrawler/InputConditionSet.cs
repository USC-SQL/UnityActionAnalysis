using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class InputConditionSet : HashSet<InputCondition>
    {
        
        public IEnumerator Perform(InputSimulator sim, InputManagerSettings inputManagerSettings, MonoBehaviour context)
        {
            List<Coroutine> coroutines = new List<Coroutine>();
            foreach (InputCondition cond in this)
            {
                coroutines.Add(context.StartCoroutine(cond.PerformInput(sim, inputManagerSettings)));
            }
            foreach (Coroutine coro in coroutines)
            {
                yield return coro;
            }
            yield break;
        }

    }
}
