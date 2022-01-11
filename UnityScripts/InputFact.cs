using UnityEngine;

public class InputFact
{

}

public class AxisInputFact : InputFact
{
    public readonly string axisName;
    public readonly float value;

    public AxisInputFact(string axisName, float value)
    {
        this.axisName = axisName;
        this.value = value;
    }

    public override string ToString()
    {
        return "Input.GetAxis(\"" + axisName + "\") == " + value;
    }
}

public class KeyDownInputFact : InputFact
{
    public readonly KeyCode keyCode;
    public readonly bool isDown;

    public KeyDownInputFact(KeyCode keyCode, bool isDown)
    {
        this.keyCode = keyCode;
        this.isDown = isDown;
    }

    public override string ToString()
    {
        return "Input.GetKeyDown(" + keyCode + ") == " + isDown;
    }
}