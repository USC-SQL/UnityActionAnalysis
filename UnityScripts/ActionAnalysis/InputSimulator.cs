using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityActionAnalysis
{
    public abstract class InputSimulator : IDisposable
    {
        protected InputManagerSettings inputManagerSettings;
        protected MonoBehaviour context;

        public InputSimulator(InputManagerSettings inputManagerSettings, MonoBehaviour context)
        {
            this.inputManagerSettings = inputManagerSettings;
            this.context = context;
        }

        public abstract void Reset();
        public abstract void SimulateDown(KeyCode keyCode);
        public abstract void SimulateUp(KeyCode keyCode);

        public IEnumerator PerformAction(InputConditionSet inputConditions)
        {
            Reset();
            List<Coroutine> coroutines = new List<Coroutine>();
            foreach (InputCondition cond in inputConditions)
            {
                coroutines.Add(context.StartCoroutine(cond.PerformInput(this, inputManagerSettings)));
            }
            foreach (Coroutine coro in coroutines)
            {
                yield return coro;
            }
            yield break;
        }

        public virtual void Dispose()
        {
        }
    }
}