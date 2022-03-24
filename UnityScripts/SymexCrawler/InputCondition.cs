using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
{
    public abstract class InputCondition
    {
        public abstract IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings);
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

        public override IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings)
        {
            List<KeyCode> keyCodesUp = new List<KeyCode>();
            KeyCode? keyCodeDown = null;

            KeyCode? positiveKey = inputManagerSettings.GetPositiveKey(axisName);
            KeyCode? negativeKey = inputManagerSettings.GetNegativeKey(axisName);

            if (value > 0.0f)
            {
                if (positiveKey.HasValue)
                {
                    keyCodeDown = positiveKey.Value;
                }
                if (negativeKey.HasValue)
                {
                    keyCodesUp.Add(negativeKey.Value);
                }
            } else if (value < 0.0f)
            {
                if (negativeKey.HasValue)
                {
                    keyCodeDown = negativeKey.Value;
                }
                if (positiveKey.HasValue)
                {
                    keyCodesUp.Add(positiveKey.Value);
                }
            } else
            {
                if (positiveKey.HasValue)
                {
                    keyCodesUp.Add(positiveKey.Value);
                }
                if (negativeKey.HasValue)
                {
                    keyCodesUp.Add(negativeKey.Value);
                }
            }
            foreach (var keyCode in keyCodesUp)
            {
                sim.SimulateUp(keyCode);
            }
            if (keyCodeDown != null)
            {
                sim.SimulateDown(keyCodeDown.Value);
            }
            yield break;
        }
    }

    public class ButtonInputCondition : InputCondition
    {
        public readonly string buttonName;
        public readonly bool isDown;

        public ButtonInputCondition(string buttonName, bool isDown)
        {
            this.buttonName = buttonName;
            this.isDown = isDown;
        }

        public override string ToString()
        {
            return "Input.GetButton(\"" + buttonName + "\") == " + isDown;
        }

        public override IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings)
        {
            KeyCode? positiveKey = inputManagerSettings.GetPositiveKey(buttonName);
            if (positiveKey.HasValue)
            {
                var keyCode = positiveKey.Value;
                if (isDown)
                {
                    sim.SimulateDown(keyCode);
                } else
                {
                    sim.SimulateUp(keyCode);
                }
            } else
            {
                Debug.LogWarning("failed to perform button input, no positive key found for: " + buttonName);
            }
            yield break;
        }
    }

    public class ButtonDownInputCondition : InputCondition
    {
        public readonly string buttonName;
        public readonly bool isDown;

        public ButtonDownInputCondition(string buttonName, bool isDown)
        {
            this.buttonName = buttonName;
            this.isDown = isDown;
        }

        public override string ToString()
        {
            return "Input.GetButtonDown(\"" + buttonName + "\") == " + isDown;
        }

        public override IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings)
        {
            KeyCode? positiveKey = inputManagerSettings.GetPositiveKey(buttonName);
            if (positiveKey.HasValue)
            {
                var keyCode = positiveKey.Value;
                if (isDown)
                {
                    sim.SimulateUp(keyCode);
                    yield return new WaitForSecondsRealtime(0.01f);
                    sim.SimulateDown(keyCode);
                }
                else if (Input.GetKey(keyCode))
                {
                    sim.SimulateUp(keyCode);
                }
            }
            else
            {
                Debug.LogWarning("failed to perform button input, no positive key found for: " + buttonName);
            }
            yield break;
        }
    }

    public class ButtonUpInputCondition : InputCondition
    {
        public readonly string buttonName;
        public readonly bool isUp;

        public ButtonUpInputCondition(string buttonName, bool isUp)
        {
            this.buttonName = buttonName;
            this.isUp = isUp;
        }

        public override string ToString()
        {
            return "Input.GetButtonUp(\"" + buttonName + "\") == " + isUp;
        }

        public override IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings)
        {
            KeyCode? positiveKey = inputManagerSettings.GetPositiveKey(buttonName);
            if (positiveKey.HasValue)
            {
                var keyCode = positiveKey.Value;
                if (isUp)
                {
                    sim.SimulateDown(keyCode);
                    yield return new WaitForSecondsRealtime(0.01f);
                    sim.SimulateUp(keyCode);
                }
                else
                {
                    sim.SimulateDown(keyCode);
                }
            }
            else
            {
                Debug.LogWarning("failed to perform button input, no positive key found for: " + buttonName);
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
            return "Input.GetKey(KeyCode." + keyCode + ") == " + isDown;
        }
        public override IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings)
        {
            if (isDown)
            {
                sim.SimulateDown(keyCode);
            }
            else
            {
                sim.SimulateUp(keyCode);
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
            return "Input.GetKeyDown(KeyCode." + keyCode + ") == " + isDown;
        }

        public override IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings)
        {
            if (isDown)
            {
                sim.SimulateUp(keyCode);
                yield return new WaitForSecondsRealtime(0.01f);
                sim.SimulateDown(keyCode);
            }
            else
            {
                sim.SimulateUp(keyCode);
            }
            yield break;
        }
    }

    public class KeyUpInputCondition : InputCondition
    {
        public readonly KeyCode keyCode;
        public readonly bool isUp;

        public KeyUpInputCondition(KeyCode keyCode, bool isUp)
        {
            this.keyCode = keyCode;
            this.isUp = isUp;
        }

        public override string ToString()
        {
            return "Input.GetKeyUp(KeyCode." + keyCode + ") == " + isUp;
        }

        public override IEnumerator PerformInput(InputSimulator sim, InputManagerSettings inputManagerSettings)
        {
            if (isUp)
            {
                sim.SimulateDown(keyCode);
                yield return new WaitForSecondsRealtime(0.01f);
                sim.SimulateUp(keyCode);
            } else if (!Input.GetKey(keyCode))
            {
                sim.SimulateDown(keyCode);
            }
            yield break;
        }
    }
}