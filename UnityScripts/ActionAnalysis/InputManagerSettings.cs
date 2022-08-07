using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;

namespace UnityActionAnalysis
{
    public enum InputManagerMode
    {
        KEYBOARD,
        JOYSTICK
    }

    public class InputManagerAxis
    {
        public KeyCode? positiveKeyCode;
        public KeyCode? negativeKeyCode;

        public InputManagerAxis(KeyCode? positiveKeyCode, KeyCode? negativeKeyCode)
        {
            this.positiveKeyCode = positiveKeyCode;
            this.negativeKeyCode = negativeKeyCode;
        }
    }

    public class InputManagerSettings
    {
        private Dictionary<string, InputManagerAxis> axes;
        private InputManagerMode mode;

        private class InputManagerAxisData
        {
            public string m_Name;
            public string positiveButton;
            public string negativeButton;
            public int type;
            public int axis;
        }

        private class InputManagerData
        {
            public List<InputManagerAxisData> m_Axes;
        }

        private class InputManagerParsed
        {
            public InputManagerData InputManager;
        }

        public InputManagerSettings(string settingsPath, InputManagerMode mode)
        {
            axes = new Dictionary<string, InputManagerAxis>();
            this.mode = mode;

            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

            InputManagerData data;
            using (var sr = new StreamReader(File.OpenRead(settingsPath)))
            {
                StringBuilder sb = new StringBuilder();
                for (; ;)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (line.StartsWith("---"))
                    {
                        sb.AppendLine("---");
                    } else if (!line.StartsWith("%TAG"))
                    {
                        sb.AppendLine(line);
                    }
                }
                try
                {
                    data = deserializer.Deserialize<InputManagerParsed>(sb.ToString()).InputManager;
                } catch (YamlDotNet.Core.SemanticErrorException e)
                {
                    throw new Exception("Could not parse InputManager.asset; make sure asset serialization mode is set to \"Force Text\"");
                }
            }
            foreach (var axisData in data.m_Axes)
            {
                if (!IncludeAxisData(axisData))
                {
                    continue;
                }
                KeyCode? positiveKeyCode;
                KeyCode? negativeKeyCode;

                if (axisData.type == 0) // Key Button
                {
                    if (axisData.positiveButton != null && axisData.positiveButton.Length > 0)
                    {
                        positiveKeyCode = KeyNameToCode(axisData.positiveButton);
                    }
                    else
                    {
                        positiveKeyCode = null;
                    }
                    if (axisData.negativeButton != null && axisData.negativeButton.Length > 0)
                    {
                        negativeKeyCode = KeyNameToCode(axisData.negativeButton);
                    }
                    else
                    {
                        negativeKeyCode = null;
                    }
                } else if (axisData.type == 2) // Joystick Axis
                {
                    switch (axisData.axis)
                    {
                        case 0: // LS X
                            positiveKeyCode = KeyCode.RightArrow;
                            negativeKeyCode = KeyCode.LeftArrow;
                            break;
                        case 1: // LS Y
                            positiveKeyCode = KeyCode.UpArrow;
                            negativeKeyCode = KeyCode.DownArrow;
                            break;
                        case 3: // RS X
                            positiveKeyCode = KeyCode.JoystickButton13;
                            negativeKeyCode = KeyCode.JoystickButton12;
                            break;
                        case 4: // RS Y
                            positiveKeyCode = KeyCode.JoystickButton15;
                            negativeKeyCode = KeyCode.JoystickButton14;
                            break;
                        case 5: // DPAD X
                            positiveKeyCode = KeyCode.JoystickButton17;
                            negativeKeyCode = KeyCode.JoystickButton16;
                            break;
                        case 6: // DPAD Y
                            positiveKeyCode = KeyCode.JoystickButton19;
                            negativeKeyCode = KeyCode.JoystickButton18;
                            break;
                        case 8: // LT
                            positiveKeyCode = KeyCode.JoystickButton10;
                            negativeKeyCode = null;
                            break;
                        case 9: // RT
                            positiveKeyCode = KeyCode.JoystickButton11;
                            negativeKeyCode = null;
                            break;
                        default:
                            positiveKeyCode = null;
                            negativeKeyCode = null;
                            break;
                    }
                } else
                {
                    positiveKeyCode = null;
                    negativeKeyCode = null;
                }

                if (axes.TryGetValue(axisData.m_Name, out var axis))
                {
                    if (!axis.positiveKeyCode.HasValue)
                    {
                        axis.positiveKeyCode = positiveKeyCode;
                    }
                    if (!axis.negativeKeyCode.HasValue)
                    {
                        axis.negativeKeyCode = negativeKeyCode;
                    }
                } else
                {
                    axes.Add(axisData.m_Name, new InputManagerAxis(positiveKeyCode, negativeKeyCode));
                }
            }
        }

        private bool IncludeAxisData(InputManagerAxisData axisData)
        {
            if (mode == InputManagerMode.KEYBOARD)
            {
                return axisData.type == 0 &&
                       (axisData.positiveButton == null || axisData.positiveButton.Length == 0 || !axisData.positiveButton.Contains("joystick"))
                    && (axisData.negativeButton == null || axisData.negativeButton.Length == 0 || !axisData.negativeButton.Contains("joystick")); 
            } else // mode == InputManagerMode.JOYSTICK
            {
                return axisData.type == 2 ||
                    (axisData.type == 0 && (
                        (axisData.positiveButton != null && axisData.positiveButton.Contains("joystick")) || 
                        (axisData.negativeButton != null && axisData.negativeButton.Contains("joystick"))));
            }
        }

        public static KeyCode? KeyNameToCode(string buttonName)
        {
            if (buttonName.StartsWith("joystick button"))
            {
                return (KeyCode)Enum.Parse(typeof(KeyCode), "JoystickButton" + int.Parse(buttonName.Replace("joystick button", "").Trim()));
            } else if (buttonName.Contains("ctrl"))
            {
                return KeyCode.LeftControl;
            }
            else if (buttonName.Contains("alt"))
            {
                return KeyCode.LeftAlt;
            }
            else if (buttonName.Contains("shift"))
            {
                return KeyCode.LeftShift;
            }
            else if (buttonName.Contains("cmd")) 
            {
                return KeyCode.LeftWindows;
            }
            else if (buttonName.Contains("enter"))
            {
                return KeyCode.Return;
            }
            else
            {
                return Event.KeyboardEvent(buttonName).keyCode;
            }
        }

        public KeyCode? GetPositiveKey(string axisName)
        {
            if (axes.TryGetValue(axisName, out var axis))
            {
                return axis.positiveKeyCode;
            } else
            {
                return null;
            }
        }

        public KeyCode? GetNegativeKey(string axisName)
        {
            if (axes.TryGetValue(axisName, out var axis))
            {
                return axis.negativeKeyCode;
            } else
            {
                return null;
            }
        }
    }
}
