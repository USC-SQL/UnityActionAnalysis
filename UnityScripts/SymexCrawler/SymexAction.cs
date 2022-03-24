using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class SymexAction
    {
        public readonly SymexPath path;
        private readonly MonoBehaviour instance;
        private readonly ISet<InputCondition> contextConditions;

        public SymexAction(SymexPath path, MonoBehaviour instance, ISet<InputCondition> contextConditions)
        {
            this.path = path;
            this.instance = instance;
            this.contextConditions = contextConditions;
        }

        public InputConditionSet TrySolve()
        {
            if (path.SolveForInputs(instance, out InputConditionSet inputConditions))
            {
                foreach (InputCondition cond in contextConditions)
                {
                    inputConditions.Add(cond);
                }
                return inputConditions;
            } else
            {
                return null;
            }
        }
    }
}
