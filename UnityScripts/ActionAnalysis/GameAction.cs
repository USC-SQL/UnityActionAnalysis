using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityActionAnalysis
{
    public class GameAction
    {
        public readonly SymexPath path;
        private readonly MonoBehaviour instance;
        private readonly ISet<InputCondition> contextConditions;

        public GameAction(SymexPath path, MonoBehaviour instance, ISet<InputCondition> contextConditions)
        {
            this.path = path;
            this.instance = instance;
            this.contextConditions = contextConditions;
        }

        public bool TrySolve(out InputConditionSet inputConditions)
        {
            if (path.SolveForInputs(instance, out inputConditions))
            {
                return true;
            } else
            {
                inputConditions = null;
                return false;
            }
        }
    }
}
