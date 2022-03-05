using UnityEngine;
using WindowsInput.Native;
using WindowsInputSimulator = WindowsInput.InputSimulator;

namespace UnitySymexCrawler
{
    public class KeyboardInputSimulator : InputSimulator
    {
        private WindowsInputSimulator sim = new WindowsInputSimulator();

        private VirtualKeyCode ConvertUnityKeyCode(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Backspace:
                    return VirtualKeyCode.BACK;
                case KeyCode.Return:
                    return VirtualKeyCode.RETURN;
                case KeyCode.Pause:
                    return VirtualKeyCode.PAUSE;
                case KeyCode.LeftArrow:
                    return VirtualKeyCode.LEFT;
                case KeyCode.RightArrow:
                    return VirtualKeyCode.RIGHT;
                case KeyCode.UpArrow:
                    return VirtualKeyCode.UP;
                case KeyCode.DownArrow:
                    return VirtualKeyCode.DOWN;
                case KeyCode.Escape:
                    return VirtualKeyCode.ESCAPE;
                case KeyCode.Space:
                    return VirtualKeyCode.SPACE;
                case KeyCode.LeftControl:
                    return VirtualKeyCode.LCONTROL;
                case KeyCode.LeftShift:
                    return VirtualKeyCode.LSHIFT;
                case KeyCode.LeftAlt:
                    return VirtualKeyCode.MENU;
                case KeyCode.RightControl:
                    return VirtualKeyCode.RCONTROL;
                case KeyCode.RightShift:
                    return VirtualKeyCode.RSHIFT;
                case KeyCode.Comma:
                    return VirtualKeyCode.OEM_COMMA;
                case KeyCode.Minus:
                    return VirtualKeyCode.OEM_MINUS;
                case KeyCode.Period:
                    return VirtualKeyCode.OEM_PERIOD;
                case KeyCode.Alpha0:
                    return VirtualKeyCode.VK_0;
                case KeyCode.Alpha1:
                    return VirtualKeyCode.VK_1;
                case KeyCode.Alpha2:
                    return VirtualKeyCode.VK_2;
                case KeyCode.Alpha3:
                    return VirtualKeyCode.VK_3;
                case KeyCode.Alpha4:
                    return VirtualKeyCode.VK_4;
                case KeyCode.Alpha5:
                    return VirtualKeyCode.VK_5;
                case KeyCode.Alpha6:
                    return VirtualKeyCode.VK_6;
                case KeyCode.Alpha7:
                    return VirtualKeyCode.VK_7;
                case KeyCode.Alpha8:
                    return VirtualKeyCode.VK_8;
                case KeyCode.Alpha9:
                    return VirtualKeyCode.VK_9;
                case KeyCode.Delete:
                    return VirtualKeyCode.DELETE;
                case KeyCode.Keypad0:
                    return VirtualKeyCode.NUMPAD0;
                case KeyCode.Keypad1:
                    return VirtualKeyCode.NUMPAD1;
                case KeyCode.Keypad2:
                    return VirtualKeyCode.NUMPAD2;
                case KeyCode.Keypad3:
                    return VirtualKeyCode.NUMPAD3;
                case KeyCode.Keypad4:
                    return VirtualKeyCode.NUMPAD4;
                case KeyCode.Keypad5:
                    return VirtualKeyCode.NUMPAD5;
                case KeyCode.Keypad6:
                    return VirtualKeyCode.NUMPAD6;
                case KeyCode.Keypad7:
                    return VirtualKeyCode.NUMPAD7;
                case KeyCode.Keypad8:
                    return VirtualKeyCode.NUMPAD8;
                case KeyCode.Keypad9:
                    return VirtualKeyCode.NUMPAD9;
                case KeyCode.Insert:
                    return VirtualKeyCode.INSERT;
                case KeyCode.Home:
                    return VirtualKeyCode.HOME;
                case KeyCode.End:
                    return VirtualKeyCode.END;
                case KeyCode.F1:
                    return VirtualKeyCode.F1;
                case KeyCode.F2:
                    return VirtualKeyCode.F2;
                case KeyCode.F3:
                    return VirtualKeyCode.F3;
                case KeyCode.F4:
                    return VirtualKeyCode.F4;
                case KeyCode.F5:
                    return VirtualKeyCode.F5;
                case KeyCode.F6:
                    return VirtualKeyCode.F6;
                case KeyCode.F7:
                    return VirtualKeyCode.F7;
                case KeyCode.F8:
                    return VirtualKeyCode.F8;
                case KeyCode.F9:
                    return VirtualKeyCode.F9;
                case KeyCode.F10:
                    return VirtualKeyCode.F10;
                case KeyCode.F11:
                    return VirtualKeyCode.F11;
                case KeyCode.F12:
                    return VirtualKeyCode.F12;
                case KeyCode.F13:
                    return VirtualKeyCode.F13;
                case KeyCode.F14:
                    return VirtualKeyCode.F14;
                case KeyCode.F15:
                    return VirtualKeyCode.F15;
                case KeyCode.Help:
                    return VirtualKeyCode.HELP;
                default:
                    if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z)
                    {
                        return (VirtualKeyCode)((int)keyCode - 32);
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        public override void SimulateDown(KeyCode keyCode)
        {
            var winKeyCode = ConvertUnityKeyCode(keyCode);
            if (winKeyCode > 0)
            {
                // Debug.Log("SimulateKeyDown: " + winKeyCode);
                sim.Keyboard.KeyDown(winKeyCode);
            }
            else
            {
                Debug.LogWarning("failed to simulate key down, unrecognized key code: " + keyCode);
            }
        }

        public override void SimulateUp(KeyCode keyCode)
        {
            var winKeyCode = ConvertUnityKeyCode(keyCode);
            if (winKeyCode > 0)
            {
                // Debug.Log("SimulateKeyUp: " + winKeyCode);
                sim.Keyboard.KeyUp(winKeyCode);
            }
            else
            {
                Debug.LogWarning("failed to simulate key up, unrecognized key code: " + keyCode);
            }
        }

        public override void Dispose()
        {
        }
    }
}
