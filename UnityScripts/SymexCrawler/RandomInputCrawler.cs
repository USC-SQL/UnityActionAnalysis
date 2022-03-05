using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class RandomInputCrawler : MonoBehaviour
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
            KeyCode.LeftArrow
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
        
        private List<KeyCode> keyCodes;

        private InputSimulator inputSim;

        private void Start()
        {
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
            StartCoroutine("CrawlLoop");
        }

        public IEnumerator CrawlLoop()
        {
            yield return new WaitForSeconds(Interval);
            for (; ;)
            {
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
                yield return new WaitForFixedUpdate();
                foreach (KeyCode keyCode in keyCodesToPress)
                {
                    inputSim.SimulateDown(keyCode);
                }
                yield return new WaitForSeconds(Interval);
                foreach(KeyCode keyCode in keyCodesToPress)
                {
                    inputSim.SimulateUp(keyCode);
                }
            }
        }
    }
}
