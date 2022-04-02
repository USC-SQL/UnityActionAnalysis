using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class RandomInputCrawler : MonoBehaviour, ICrawler
    {
        private static readonly List<KeyCode> DefaultKeyboardKeyCodes = new List<KeyCode>() 
        {
            KeyCode.Backspace,
            KeyCode.Return,
            KeyCode.Pause,
            KeyCode.Escape,
            KeyCode.Space,
            KeyCode.Comma,
            KeyCode.Minus,
            KeyCode.Period,
            KeyCode.Alpha0,
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.A,
            KeyCode.B,
            KeyCode.C,
            KeyCode.D,
            KeyCode.E,
            KeyCode.F,
            KeyCode.G,
            KeyCode.H,
            KeyCode.I,
            KeyCode.J,
            KeyCode.K,
            KeyCode.L,
            KeyCode.M,
            KeyCode.N,
            KeyCode.O,
            KeyCode.P,
            KeyCode.Q,
            KeyCode.R,
            KeyCode.S,
            KeyCode.T,
            KeyCode.U,
            KeyCode.V,
            KeyCode.W,
            KeyCode.X,
            KeyCode.Y,
            KeyCode.Z,
            KeyCode.Delete,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.RightArrow,
            KeyCode.LeftArrow,
            KeyCode.Home,
            KeyCode.End,
            KeyCode.F1,
            KeyCode.F2,
            KeyCode.F3,
            KeyCode.F4,
            KeyCode.F5,
            KeyCode.F6,
            KeyCode.F7,
            KeyCode.F8,
            KeyCode.F9,
            KeyCode.F10,
            KeyCode.F11,
            KeyCode.F12
        };

        private static readonly List<KeyCode> DefaultJoystickKeyCodes = new List<KeyCode>() 
        {
            KeyCode.JoystickButton0,
            KeyCode.JoystickButton1,
            KeyCode.JoystickButton2,
            KeyCode.JoystickButton3,
            KeyCode.JoystickButton4,
            KeyCode.JoystickButton5,
            KeyCode.JoystickButton6,
            KeyCode.JoystickButton7,
            KeyCode.JoystickButton8,
            KeyCode.JoystickButton9,
            KeyCode.JoystickButton10,
            KeyCode.JoystickButton11,
            KeyCode.JoystickButton12,
            KeyCode.JoystickButton13,
            KeyCode.JoystickButton14,
            KeyCode.JoystickButton15,
            KeyCode.JoystickButton16,
            KeyCode.JoystickButton17,
            KeyCode.JoystickButton18,
            KeyCode.JoystickButton19,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.DownArrow,
            KeyCode.UpArrow
        };

        public float Interval = 0.1f;
        public bool Joystick = false;
        public int MinNumButtonsToPress = 1;
        public int MaxNumButtonsToPress = 3;

        public List<string> ExcludeKeys = new List<string>();

        // if empty, all default keys included
        public List<string> IncludeKeys = new List<string>();

        // keys to include in addition to default keys
        public List<string> AdditionalKeys = new List<string>();

        // JSON file to write statistics to
        public string StatisticsOutputFile = "";
        
        private List<KeyCode> keyCodes;
        private InputSimulator inputSim;
        private int numActionsPerformed;
        private string runId;
        private bool isPaused;

        private void Start()
        {
            DontDestroyOnLoad(this);

            numActionsPerformed = 0;
            var stateDumper = (UnityStateDumper.StateDumper)FindObjectOfType(typeof(UnityStateDumper.StateDumper));
            runId = stateDumper.runId;
            isPaused = false;

            inputSim = Joystick ? (InputSimulator)new JoystickInputSimulator() : new KeyboardInputSimulator();

            keyCodes = new List<KeyCode>();
            List<KeyCode> allKeyCodes;
            if (IncludeKeys.Count > 0)
            {
                List<KeyCode> kcs = new List<KeyCode>();
                foreach (string key in IncludeKeys)
                {
                    kcs.Add((KeyCode)Enum.Parse(typeof(KeyCode), key));
                }
                allKeyCodes = kcs;
            } else
            {
                allKeyCodes = Joystick ? DefaultJoystickKeyCodes : DefaultKeyboardKeyCodes;
            }
            foreach (KeyCode keyCode in allKeyCodes)
            {
                if (ExcludeKeys.Contains(keyCode.ToString()))
                {
                    continue;
                }
                keyCodes.Add(keyCode);
            }
            foreach (string keyCode in AdditionalKeys)
            {
                keyCodes.Add((KeyCode)Enum.Parse(typeof(KeyCode), keyCode));
            }

            StartCoroutine("CrawlLoop");
        }

        public IEnumerator CrawlLoop()
        {
            List<KeyCode> lastKeyCodesPressed = null;
            float lastActionTime = Time.realtimeSinceStartup;
            for (; ;)
            {
                if (isPaused || Time.realtimeSinceStartup - lastActionTime < Interval)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                if (lastKeyCodesPressed != null)
                {
                    foreach (KeyCode keyCode in lastKeyCodesPressed)
                    {
                        inputSim.SimulateUp(keyCode);
                    }
                    lastKeyCodesPressed = null;
                }
                List<KeyCode> keyCodesToPress = new List<KeyCode>();
                int numToPress = UnityEngine.Random.Range(MinNumButtonsToPress, MaxNumButtonsToPress + 1);
                for (int i = 0; i < numToPress; ++i)
                {
                    keyCodesToPress.Add(keyCodes[UnityEngine.Random.Range(0, keyCodes.Count)]);
                }
                Debug.Log("Pressing " + string.Join(", ", keyCodesToPress.Select(kc => kc.ToString())));
                foreach (KeyCode keyCode in keyCodesToPress)
                {
                    inputSim.SimulateUp(keyCode);
                }
                yield return new WaitForSecondsRealtime(0.01f);
                foreach (KeyCode keyCode in keyCodesToPress)
                {
                    inputSim.SimulateDown(keyCode);
                }
                lastKeyCodesPressed = keyCodesToPress;
                ++numActionsPerformed;
                lastActionTime = Time.realtimeSinceStartup;
                yield return new WaitForEndOfFrame();
            }
        }

        private void OnDestroy()
        {
            try
            {
                if (StatisticsOutputFile.Length > 0)
                {
                    using (var sw = new StreamWriter(File.OpenWrite(StatisticsOutputFile + "." + runId + ".json")))
                    {
                        sw.Write(JsonConvert.SerializeObject(new
                        {
                            NumActionsPerformed = numActionsPerformed
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
            }
            if (inputSim != null)
            {
                inputSim.Reset();
                inputSim.Dispose();
            }
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
        }
    }
}
