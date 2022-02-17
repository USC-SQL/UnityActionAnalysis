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
            else if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z)
            {
                return (VirtualKeyCode)((int)keyCode - 32);
            }
            else
            {
                throw new Exception("unrecognized key code: " + keyCode);
            }
        }

        public void SimulateKeyDown(KeyCode keyCode)
        {
            var winKeyCode = ConvertUnityKeyCode(keyCode);
            Debug.Log("SimulateKeyDown: " + winKeyCode);
            sim.Keyboard.KeyDown(winKeyCode);
        }

        public void SimulateKeyUp(KeyCode keyCode)
        {
            var winKeyCode = ConvertUnityKeyCode(keyCode);
            Debug.Log("SimulateKeyUp: " + winKeyCode);
            sim.Keyboard.KeyUp(winKeyCode);
        }
    }
}