using System;
using UnityEngine;
using vJoyInterfaceWrap;

namespace UnityActionAnalysis
{
    /**
     * Expects vJoy and x360ce installed, with a 24-button vJoy controller with ID 1
     * and the following x360ce preset:
       <PadSetting>
         <PadSettingChecksum>1c62bdd7-0e04-0a34-0e04-7f9a5f2658fc</PadSettingChecksum>
         <ButtonA>1</ButtonA>
         <ButtonB>2</ButtonB>
         <ButtonBack>7</ButtonBack>
         <ButtonStart>8</ButtonStart>
         <ButtonX>3</ButtonX>
         <ButtonY>4</ButtonY>
         <DPadDown>19</DPadDown>
         <DPadLeft>17</DPadLeft>
         <DPadRight>18</DPadRight>
         <DPadUp>20</DPadUp>
         <GamePadType>1</GamePadType>
         <LeftMotorPeriod>60</LeftMotorPeriod>
         <LeftShoulder>5</LeftShoulder>
         <LeftThumbButton>9</LeftThumbButton>
         <LeftThumbDown>23</LeftThumbDown>
         <LeftThumbLeft>21</LeftThumbLeft>
         <LeftThumbRight>22</LeftThumbRight>
         <LeftThumbUp>24</LeftThumbUp>
         <LeftTrigger>11</LeftTrigger>
         <RightMotorPeriod>120</RightMotorPeriod>
         <RightShoulder>6</RightShoulder>
         <RightThumbButton>10</RightThumbButton>
         <RightThumbDown>15</RightThumbDown>
         <RightThumbLeft>13</RightThumbLeft>
         <RightThumbRight>14</RightThumbRight>
         <RightThumbUp>16</RightThumbUp>
         <RightTrigger>12</RightTrigger>
       </PadSetting>
     */
    public class JoystickInputSimulator : InputSimulator
    {
        private const int RID = 1;

        private vJoy joystick;

        public JoystickInputSimulator(InputManagerSettings inputManagerSettings, MonoBehaviour context) :
            base(inputManagerSettings, context)
        {
            joystick = new vJoy();
            if (!joystick.vJoyEnabled())
            {
                throw new Exception("vJoy not available");
            }

            VjdStat status = joystick.GetVJDStatus(RID);
            if (status == VjdStat.VJD_STAT_OWN)
            {
                joystick.RelinquishVJD(RID);
            } else if (status != VjdStat.VJD_STAT_FREE)
            {
                throw new Exception("vJoy joystick " + RID + " not available due to " + status);
            }

            joystick.AcquireVJD(RID);
            joystick.ResetVJD(RID);

            joystick.ResetButtons(RID);
        }

        private int? GetButtonId(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.LeftArrow:
                    return 21;
                case KeyCode.RightArrow:
                    return 22;
                case KeyCode.DownArrow:
                    return 23;
                case KeyCode.UpArrow:
                    return 24;
                default:
                    if (keyCode >= KeyCode.JoystickButton0 && keyCode < KeyCode.Joystick1Button0)
                    {
                        return keyCode - KeyCode.JoystickButton0 + 1;
                    }
                    else if (keyCode >= KeyCode.Joystick1Button0 && keyCode < KeyCode.Joystick2Button0)
                    {
                        return keyCode - KeyCode.Joystick1Button0 + 1;
                    }
                    else if (keyCode >= KeyCode.Joystick2Button0 && keyCode < KeyCode.Joystick3Button0)
                    {
                        return keyCode - KeyCode.Joystick2Button0 + 1;
                    }
                    else
                    {
                        return null;
                    }
            }
        }

        public override void Reset()
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                int? btn = GetButtonId(keyCode);
                if (btn.HasValue)
                {
                    joystick.SetBtn(false, RID, (uint)btn.Value);
                }
            }
        }

        public override void SimulateDown(KeyCode keyCode)
        {
            int? btn = GetButtonId(keyCode);
            if (btn.HasValue)
            {
                joystick.SetBtn(true, RID, (uint)btn.Value);
            } else
            {
                Debug.LogWarning("unrecognized key code for joystick: " + keyCode);
            }
        }

        public override void SimulateUp(KeyCode keyCode)
        {
            int? btn = GetButtonId(keyCode);
            if (btn.HasValue)
            {
                joystick.SetBtn(false, RID, (uint)btn.Value);
            } else
            {
                Debug.LogWarning("unrecognized key code for joystick: " + keyCode);
            }
        }

        public override void Dispose()
        {
            joystick.RelinquishVJD(RID);
        }
    }
}
