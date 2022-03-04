using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class RandomInputCrawler : MonoBehaviour
    {
        public float Interval = 0.1f;
        public int minNumKeysToPress = 1;
        public int maxNumKeysToPress = 3;

        public List<string> ExcludeKeys = new List<string>() {
            KeyCode.None.ToString(),
            KeyCode.Tab.ToString(),
            KeyCode.LeftWindows.ToString(),
            KeyCode.RightWindows.ToString(),
            KeyCode.LeftCommand.ToString(),
            KeyCode.LeftApple.ToString(),
            KeyCode.RightCommand.ToString(),
            KeyCode.RightApple.ToString(),
            KeyCode.SysReq.ToString(),
            KeyCode.Print.ToString(),
            KeyCode.CapsLock.ToString(),
            KeyCode.Numlock.ToString(),
            KeyCode.ScrollLock.ToString(),
            KeyCode.Menu.ToString(),
            KeyCode.Clear.ToString(),
            KeyCode.Exclaim.ToString(),
            KeyCode.DoubleQuote.ToString(),
            KeyCode.Hash.ToString(),
            KeyCode.Dollar.ToString(),
            KeyCode.Percent.ToString(),
            KeyCode.Ampersand.ToString(),
            KeyCode.Quote.ToString(),
            KeyCode.LeftParen.ToString(),
            KeyCode.RightParen.ToString(),
            KeyCode.Asterisk.ToString(),
            KeyCode.Plus.ToString(),
            KeyCode.Slash.ToString(),
            KeyCode.Colon.ToString(),
            KeyCode.Semicolon.ToString(),
            KeyCode.Less.ToString(),
            KeyCode.Equals.ToString(),
            KeyCode.Greater.ToString(),
            KeyCode.Question.ToString(),
            KeyCode.At.ToString(),
            KeyCode.LeftBracket.ToString(),
            KeyCode.Backslash.ToString(),
            KeyCode.RightBracket.ToString(),
            KeyCode.Caret.ToString(),
            KeyCode.Underscore.ToString(),
            KeyCode.BackQuote.ToString(),
            KeyCode.KeypadPeriod.ToString(),
            KeyCode.KeypadDivide.ToString(),
            KeyCode.KeypadMultiply.ToString(),
            KeyCode.KeypadMinus.ToString(),
            KeyCode.KeypadPlus.ToString(),
            KeyCode.KeypadEnter.ToString(),
            KeyCode.KeypadEquals.ToString(),
            KeyCode.PageUp.ToString(),
            KeyCode.PageDown.ToString(),
            KeyCode.RightAlt.ToString(),
            KeyCode.AltGr.ToString(),
            KeyCode.Break.ToString(),
            KeyCode.LeftCurlyBracket.ToString(),
            KeyCode.Pipe.ToString(),
            KeyCode.RightCurlyBracket.ToString(),
            KeyCode.Tilde.ToString(),
            KeyCode.F1.ToString(),
            KeyCode.F2.ToString(),
            KeyCode.F3.ToString(),
            KeyCode.F4.ToString(),
            KeyCode.F5.ToString(),
            KeyCode.F6.ToString(),
            KeyCode.F7.ToString(),
            KeyCode.F8.ToString(),
            KeyCode.F9.ToString(),
            KeyCode.F10.ToString(),
            KeyCode.F11.ToString(),
            KeyCode.F12.ToString(),
            KeyCode.F13.ToString(),
            KeyCode.F14.ToString(),
            KeyCode.F15.ToString(),
            KeyCode.LeftControl.ToString(),
            KeyCode.RightControl.ToString(),
            KeyCode.LeftShift.ToString(),
            KeyCode.RightShift.ToString(),
            KeyCode.LeftAlt.ToString(),
            KeyCode.Keypad0.ToString(),
            KeyCode.Keypad1.ToString(),
            KeyCode.Keypad2.ToString(),
            KeyCode.Keypad3.ToString(),
            KeyCode.Keypad4.ToString(),
            KeyCode.Keypad5.ToString(),
            KeyCode.Keypad6.ToString(),
            KeyCode.Keypad7.ToString(),
            KeyCode.Keypad8.ToString(),
            KeyCode.Keypad9.ToString(),
            KeyCode.Insert.ToString(),
            KeyCode.Home.ToString(),
            KeyCode.End.ToString(),
            KeyCode.F1.ToString(),
            KeyCode.F2.ToString(),
            KeyCode.F3.ToString(),
            KeyCode.F4.ToString(),
            KeyCode.F5.ToString(),
            KeyCode.F6.ToString(),
            KeyCode.F7.ToString(),
            KeyCode.F8.ToString(),
            KeyCode.F9.ToString(),
            KeyCode.F10.ToString(),
            KeyCode.F11.ToString(),
            KeyCode.F12.ToString(),
            KeyCode.F13.ToString(),
            KeyCode.F14.ToString(),
            KeyCode.F15.ToString(),
            KeyCode.Help.ToString()
        };

        private List<KeyCode> keyCodes;

        private InputSimulator inputSim;

        private void Start()
        {
            inputSim = new InputSimulator();

            keyCodes = new List<KeyCode>();
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (keyCode >= KeyCode.Mouse0 || ExcludeKeys.Contains(keyCode.ToString()))
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
                int numToPress = UnityEngine.Random.Range(minNumKeysToPress, maxNumKeysToPress + 1);
                for (int i = 0; i < numToPress; ++i)
                {
                    keyCodesToPress.Add(keyCodes[UnityEngine.Random.Range(0, keyCodes.Count)]);
                }
                Debug.Log("Pressing " + string.Join(", ", keyCodesToPress.Select(kc => kc.ToString())));
                bool anyReleased = false;
                foreach (KeyCode keyCode in keyCodesToPress)
                {
                    if (Input.GetKey(keyCode))
                    {
                        inputSim.SimulateKeyUp(keyCode);
                        anyReleased = true;
                    }
                }
                if (anyReleased)
                {
                    yield return new WaitForFixedUpdate();
                }
                foreach (KeyCode keyCode in keyCodesToPress)
                {
                    inputSim.SimulateKeyDown(keyCode);
                }
                yield return new WaitForSeconds(Interval);
                foreach(KeyCode keyCode in keyCodesToPress)
                {
                    inputSim.SimulateKeyUp(keyCode);
                }
            }
        }
    }
}
