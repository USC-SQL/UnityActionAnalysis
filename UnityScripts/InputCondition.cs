using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
{
    public abstract class InputCondition
    {
        public abstract IEnumerator PerformInput(InputSimulator sim);
    }

    public class AxisInputCondition : InputCondition
    {
        public readonly string axisName;
        public readonly float value;

        public AxisInputCondition(string axisName, float value)
        {
            this.axisName = axisName;
            this.value = value;
        }

        public override string ToString()
        {
            return "Input.GetAxis(\"" + axisName + "\") == " + value;
        }

        public override IEnumerator PerformInput(InputSimulator sim)
        {
            List<KeyCode> keyCodesUp = new List<KeyCode>();
            KeyCode? keyCodeDown = null;
            switch (axisName)
            {
                case "Horizontal":
                    if (value > 0.0f)
                    {
                        keyCodesUp.Add(KeyCode.LeftArrow);
                        keyCodeDown = KeyCode.RightArrow;
                    }
                    else if (value < 0.0f)
                    {
                        keyCodesUp.Add(KeyCode.RightArrow);
                        keyCodeDown = KeyCode.LeftArrow;
                    }
                    else
                    {
                        keyCodesUp.Add(KeyCode.RightArrow);
                        keyCodesUp.Add(KeyCode.LeftArrow);
                    }
                    break;
                case "Vertical":
                    if (value > 0.0f)
                    {
                        keyCodesUp.Add(KeyCode.DownArrow);
                        keyCodeDown = KeyCode.UpArrow;
                    }
                    else if (value < 0.0f)
                    {
                        keyCodesUp.Add(KeyCode.UpArrow);
                        keyCodeDown = KeyCode.DownArrow;
                    }
                    else
                    {
                        keyCodesUp.Add(KeyCode.UpArrow);
                        keyCodesUp.Add(KeyCode.DownArrow);
                    }
                    break;
                default:
                    Debug.LogWarning("failed to perform input, did not recognize axisName " + axisName);
                    yield break;
            }

            foreach (var keyCode in keyCodesUp)
            {
                if (Input.GetKey(keyCode))
                {
                    sim.SimulateKeyUp(keyCode);
                }
            }
            if (keyCodeDown != null && !Input.GetKey(keyCodeDown.Value))
            {
                sim.SimulateKeyDown(keyCodeDown.Value);
            }

            yield break;
        }
    }

    public class KeyInputCondition : InputCondition
    {
        public readonly KeyCode keyCode;
        public readonly bool isDown;

        public KeyInputCondition(KeyCode keyCode, bool isDown)
        {
            this.keyCode = keyCode;
            this.isDown = isDown;
        }

        public override string ToString()
        {
            return "Input.GetKey(" + keyCode + ") == " + isDown;
        }
        public override IEnumerator PerformInput(InputSimulator sim)
        {
            if (isDown)
            {
                sim.SimulateKeyDown(keyCode);
            }
            else
            {
                sim.SimulateKeyUp(keyCode);
            }
            yield break;
        }
    }

    public class KeyDownInputCondition : InputCondition
    {
        public readonly KeyCode keyCode;
        public readonly bool isDown;

        public KeyDownInputCondition(KeyCode keyCode, bool isDown)
        {
            this.keyCode = keyCode;
            this.isDown = isDown;
        }

        public override string ToString()
        {
            return "Input.GetKeyDown(" + keyCode + ") == " + isDown;
        }

        public override IEnumerator PerformInput(InputSimulator sim)
        {
            if (isDown)
            {
                if (Input.GetKey(keyCode))
                {
                    sim.SimulateKeyUp(keyCode);
                    yield return new WaitForFixedUpdate();
                }
                sim.SimulateKeyDown(keyCode);
            }
            else if (Input.GetKey(keyCode))
            {
                sim.SimulateKeyUp(keyCode);
            }
            yield break;
        }
    }
}