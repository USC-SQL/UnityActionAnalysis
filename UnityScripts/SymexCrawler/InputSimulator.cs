using System;
using UnityEngine;
using WindowsInput.Native;
using WindowsInputSimulator = WindowsInput.InputSimulator;

namespace UnitySymexCrawler
{
    public class InputSimulator
    {
        private WindowsInputSimulator sim = new WindowsInputSimulator();

        private VirtualKeyCode ConvertUnityKeyCode(KeyCode keyCode)
        {
            if (keyCode == KeyCode.LeftArrow)
            {
                return VirtualKeyCode.LEFT;
            }
            else if (keyCode == KeyCode.RightArrow)
            {
                return VirtualKeyCode.RIGHT;
            }
            else if (keyCode == KeyCode.UpArrow)
            {
                return VirtualKeyCode.UP;
            }
            else if (keyCode == KeyCode.DownArrow)
            {
                return VirtualKeyCode.DOWN;
            }
            else if (keyCode == KeyCode.Escape)
            {
                return VirtualKeyCode.ESCAPE;
            }
            else if (keyCode == KeyCode.Space)
            {
                return VirtualKeyCode.SPACE;
            }
            else if (keyCode == KeyCode.LeftControl)
            {
                return VirtualKeyCode.LCONTROL;
            }
            else if (keyCode == KeyCode.LeftShift)
            {
                return VirtualKeyCode.LSHIFT;
            }
            else if (keyCode == KeyCode.LeftAlt)
            {
                return VirtualKeyCode.MENU;
            }
            else if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z)
            {
                return (VirtualKeyCode)((int)keyCode - 32);
            }
            else
            {
                return 0;
            }
        }

        public void SimulateKeyDown(KeyCode keyCode)
        {
            var winKeyCode = ConvertUnityKeyCode(keyCode);
            if (winKeyCode > 0)
            {
                // Debug.Log("SimulateKeyDown: " + winKeyCode);
                sim.Keyboard.KeyDown(winKeyCode);
            } else
            {
                Debug.LogWarning("failed to simulate key down, unrecognized key code: " + keyCode);
            }
        }

        public void SimulateKeyUp(KeyCode keyCode)
        {
            var winKeyCode = ConvertUnityKeyCode(keyCode);
            if (winKeyCode > 0)
            {
                // Debug.Log("SimulateKeyUp: " + winKeyCode);
                sim.Keyboard.KeyUp(winKeyCode);
            } else
            {
                Debug.LogWarning("failed to simulate key up, unrecognized key code: " + keyCode);
            }
        }
    }
}