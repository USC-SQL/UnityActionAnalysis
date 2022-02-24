using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;

namespace UnitySymexCrawler
{
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


        private class InputManagerAxisData
        {
            public string m_Name;
            public string positiveButton;
            public string negativeButton;
        }

        private class InputManagerData
        {
            public List<InputManagerAxisData> m_Axes;
        }

        private class InputManagerParsed
        {
            public InputManagerData InputManager;
        }

        public InputManagerSettings(string settingsPath)
        {
            axes = new Dictionary<string, InputManagerAxis>();

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
                    Debug.LogError("Could not parse InputManager.asset; make sure asset serialization mode is set to \"Force Text\"");
                    return;
                }
            }
            foreach (var axisData in data.m_Axes)
            {
                KeyCode? positiveKeyCode;
                KeyCode? negativeKeyCode;
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
                } else
                {
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

        public static KeyCode? KeyNameToCode(string buttonName)
        {
            if (buttonName.Contains("joystick"))
            {
                return null;
            }
            if (buttonName.Contains("ctrl"))
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
