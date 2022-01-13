using System;
using UnityEngine;
using WindowsInput.Native;
using WindowsInputSimulator = WindowsInput.InputSimulator;

public class InputSimulator
{
    private WindowsInputSimulator sim = new WindowsInputSimulator();

    private VirtualKeyCode ConvertUnityKeyCode(KeyCode keyCode)
    {
        switch (keyCode)
        {
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
            default:
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
