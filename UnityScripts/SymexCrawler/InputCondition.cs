using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
{
    public abstract class InputCondition
    {
        public abstract IEnumerator PerformInput(InputSimulator sim);

        protected bool GetButtonKeyCode(string buttonName, out KeyCode keyCode)
        {
            switch (buttonName)
            {
                case "Fire1":
                    keyCode = KeyCode.LeftControl;
                    return true;
                case "Fire2":
                    keyCode = KeyCode.LeftAlt;
                    return true;
                default:
                    keyCode = 0;
                    return false;
            }
        }
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
            if (GetButtonKeyCode(axisName, out KeyCode buttonCode))
            {
                if (value > 0.0f)
                {
                    sim.SimulateKeyDown(buttonCode);
                }
                else
                {
                    sim.SimulateKeyUp(buttonCode);
                }
            }
            else
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
                        Debug.LogWarning("failed to perform axis input, did not recognize axisName " + axisName);
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

        public override IEnumerator PerformInput(InputSimulator sim)
        {
            if (GetButtonKeyCode(buttonName, out KeyCode keyCode))
            {
                if (isDown)
                {
                    sim.SimulateKeyDown(keyCode);
                }
                else
                {
                    sim.SimulateKeyUp(keyCode);
                }
            } else
            {
                Debug.LogWarning("failed to perform button input, unrecognized button name: " + buttonName);
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

        public override IEnumerator PerformInput(InputSimulator sim)
        {
            if (GetButtonKeyCode(buttonName, out KeyCode keyCode))
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
            }
            else
            {
                Debug.LogWarning("failed to perform button input, unrecognized button name: " + buttonName);
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

        public override IEnumerator PerformInput(InputSimulator sim)
        {
            if (GetButtonKeyCode(buttonName, out KeyCode keyCode))
            {
                if (isUp)
                {
                    if (!Input.GetKey(keyCode))
                    {
                        sim.SimulateKeyDown(keyCode);
                        yield return new WaitForFixedUpdate();
                    }
                    sim.SimulateKeyUp(keyCode);
                }
                else if (!Input.GetKey(keyCode))
                {
                    sim.SimulateKeyDown(keyCode);
                }
            }
            else
            {
                Debug.LogWarning("failed to perform button input, unrecognized button name: " + buttonName);
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
            return "Input.GetKeyUp(" + keyCode + ") == " + isUp;
        }

        public override IEnumerator PerformInput(InputSimulator sim)
        {
            if (isUp)
            {
                if (!Input.GetKey(keyCode))
                {
                    sim.SimulateKeyDown(keyCode);
                    yield return new WaitForFixedUpdate();
                }
                sim.SimulateKeyUp(keyCode);
            } else if (!Input.GetKey(keyCode))
            {
                sim.SimulateKeyDown(keyCode);
            }
            yield break;
        }
    }
}