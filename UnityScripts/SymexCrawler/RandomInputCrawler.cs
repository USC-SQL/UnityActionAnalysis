using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySymexCrawler
{
    public class RandomInputCrawler : MonoBehaviour
    {
        public float Interval = 0.1f;

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
            KeyCode.Tilde.ToString()
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
                KeyCode keyCode = keyCodes[UnityEngine.Random.Range(0, keyCodes.Count)];
                if (Input.GetKey(keyCode))
                {
                    inputSim.SimulateKeyUp(keyCode);
                    yield return new WaitForFixedUpdate();
                }
                inputSim.SimulateKeyDown(keyCode);
                yield return new WaitForSeconds(Interval);
                inputSim.SimulateKeyUp(keyCode);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
