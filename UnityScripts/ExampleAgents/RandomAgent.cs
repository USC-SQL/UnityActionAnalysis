using System;
using System.Collections;
using System.Collections.Generic;
using UnityActionAnalysis;
using UnityEngine;

namespace UnityActionAnalysis
{
    public class RandomAgent : MonoBehaviour
    {
        public string AnalysisDatabasePath;
        public string InputManagerSettingsPath;

        public float ActionInterval = 0.1f;
        public bool UseInstrumentationInputSimulator = false;

        private ActionManager actionManager;
        private InputSimulator inputSim;

        void Start()
        {
            if (FindObjectsOfType(typeof(RandomAgent)).Length > 1)
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(this);

            if (string.IsNullOrEmpty(AnalysisDatabasePath) || string.IsNullOrEmpty(InputManagerSettingsPath))
            {
                throw new Exception("missing required parameters for RandomAgent");
            }

            actionManager = new ActionManager(AnalysisDatabasePath);
            Debug.Log(actionManager.ActionCount + " game actions");

            InputManagerSettings inputManagerSettings = new InputManagerSettings(InputManagerSettingsPath, InputManagerMode.KEYBOARD);

            if (UseInstrumentationInputSimulator)
            {
                // Can use InstrInputSimulator instead of KeyboardInputSimulator to simulate inputs without simulating actual keyboard events (see README)
                inputSim = new InstrInputSimulator(inputManagerSettings, this);
            } else
            {
                inputSim = new KeyboardInputSimulator(inputManagerSettings, this);
            }

            StartCoroutine("AgentLoop");
        }

        private bool ShouldIncludeAction(GameAction action, InputConditionSet inputConds)
        {
            // opportunity to inspect which Input invocations are involved in the input by inspecting inputConds.
            // example of skipping actions that are for a button named "Pause" or the "Escape" key (either of which are likely to be used for pausing the game)
            foreach (InputCondition cond in inputConds)
            {
                if (   (cond is ButtonInputCondition buttonCond && buttonCond.buttonName == "Pause")
                    || (cond is KeyInputCondition keyCond && keyCond.keyCode == KeyCode.Escape))
                {
                    return false;
                }
            }
            return true;
        }

        IEnumerator AgentLoop()
        {
            while (true)
            {
                Dictionary<int, GameAction> validActions = actionManager.DetermineValidActions();
                List<GameAction> actionList = new List<GameAction>(validActions.Values);
                if (actionList.Count > 0)
                {
                    GameAction chosenAction = actionList[UnityEngine.Random.Range(0, actionList.Count)];
                    if (chosenAction.TrySolve(out InputConditionSet inputConds))
                    {
                        if (ShouldIncludeAction(chosenAction, inputConds))
                        {
                            Debug.Log("Performing action: " + string.Join(" && ", inputConds));
                            yield return StartCoroutine(inputSim.PerformAction(inputConds));
                        }
                    }
                }
                yield return new WaitForSecondsRealtime(ActionInterval);
            }
        }
    }
}