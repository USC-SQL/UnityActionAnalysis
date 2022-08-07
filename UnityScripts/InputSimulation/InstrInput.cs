using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityActionAnalysis;

namespace UnityActionAnalysis
{
    public class InstrInput
    {
        private static MonoBehaviour simContext; // null if pass-through
        private static InputManagerSettings inputManagerSettings;
        private static ISet<KeyCode> newKeysDown;
        private static ISet<KeyCode> newKeysUp;
        private static ISet<KeyCode> keysHeld;
        private static Dictionary<KeyCode, Coroutine> removeNewCoroutines;

        private static bool IsPassthrough { get => simContext == null; }

        public static void SetInputManagerSettings(InputManagerSettings inputManagerSettings)
        {
            InstrInput.inputManagerSettings = inputManagerSettings;
        }

        public static void StartSimulation(MonoBehaviour context)
        {
            if (inputManagerSettings == null)
            {
                throw new Exception("set the InputManagerSettings before calling StartSimulation");
            }
            if (simContext != null)
            {
                throw new Exception("simulation already active");
            }
            simContext = context;
            newKeysDown = new HashSet<KeyCode>();
            newKeysUp = new HashSet<KeyCode>();
            keysHeld = new HashSet<KeyCode>();
            removeNewCoroutines = new Dictionary<KeyCode, Coroutine>();
        }

        public static void StopSimulation()
        {
            if (simContext == null)
            {
                return;
            }
            foreach (Coroutine coro in removeNewCoroutines.Values)
            {
                simContext.StopCoroutine(coro);
            }
            newKeysDown = null;
            newKeysUp = null;
            keysHeld = null;
            removeNewCoroutines = null;
            simContext = null;
        }

        private static IEnumerator RemoveNew(KeyCode keyCode)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            newKeysDown.Remove(keyCode);
            newKeysUp.Remove(keyCode);
        }

        public static void SimulateKeyDown(KeyCode keyCode)
        {
            if (!keysHeld.Contains(keyCode))
            {
                if (removeNewCoroutines.ContainsKey(keyCode))
                {
                    simContext.StopCoroutine(removeNewCoroutines[keyCode]);
                    removeNewCoroutines.Remove(keyCode);
                    newKeysDown.Remove(keyCode);
                    newKeysUp.Remove(keyCode);
                }
                if (!keysHeld.Contains(keyCode))
                {
                    keysHeld.Add(keyCode);
                }
                if (!newKeysDown.Contains(keyCode))
                {
                    newKeysDown.Add(keyCode);
                }
                removeNewCoroutines.Add(keyCode, simContext.StartCoroutine(RemoveNew(keyCode)));
            }
        }

        public static void SimulateKeyUp(KeyCode keyCode)
        {
            if (keysHeld.Contains(keyCode))
            {
                if (removeNewCoroutines.ContainsKey(keyCode))
                {
                    simContext.StopCoroutine(removeNewCoroutines[keyCode]);
                    removeNewCoroutines.Remove(keyCode);
                    newKeysDown.Remove(keyCode);
                    newKeysUp.Remove(keyCode);
                }
                keysHeld.Remove(keyCode);
                if (!newKeysUp.Contains(keyCode))
                {
                    newKeysUp.Add(keyCode);
                }
                removeNewCoroutines.Add(keyCode, simContext.StartCoroutine(RemoveNew(keyCode)));
            }
        }

        public static bool GetKey(string name)
        {
            if (IsPassthrough)
            {
                return Input.GetKey(name);
            }
            else
            {
                KeyCode? keyCode = InputManagerSettings.KeyNameToCode(name);
                if (keyCode.HasValue)
                {
                    return keysHeld.Contains(keyCode.Value);
                }
                else
                {
                    Debug.LogWarning("unrecognized key name '" + name + "'");
                    return false;
                }
            }
        }

        public static bool GetKey(KeyCode key)
        {
            if (IsPassthrough)
            {
                return Input.GetKey(key);
            }
            else
            {
                return keysHeld.Contains(key);
            }
        }

        public static bool GetKeyDown(string name)
        {
            if (IsPassthrough)
            {
                return Input.GetKeyDown(name);
            }
            else
            {
                KeyCode? keyCode = InputManagerSettings.KeyNameToCode(name);
                if (keyCode.HasValue)
                {
                    return newKeysDown.Contains(keyCode.Value);
                }
                else
                {
                    Debug.LogWarning("unrecognized key name '" + name + "'");
                    return false;
                }
            }
        }

        public static bool GetKeyDown(KeyCode key)
        {
            if (IsPassthrough)
            {
                return Input.GetKeyDown(key);
            }
            else
            {
                return newKeysDown.Contains(key);
            }
        }

        public static bool GetKeyUp(string name)
        {
            if (IsPassthrough)
            {
                return Input.GetKeyUp(name);
            }
            else
            {
                KeyCode? keyCode = InputManagerSettings.KeyNameToCode(name);
                if (keyCode.HasValue)
                {
                    return newKeysUp.Contains(keyCode.Value);
                }
                else
                {
                    Debug.LogWarning("unrecognized key name '" + name + "'");
                    return false;
                }
            }
        }

        public static bool GetKeyUp(KeyCode key)
        {
            if (IsPassthrough)
            {
                return Input.GetKeyUp(key);
            }
            else
            {
                return newKeysUp.Contains(key);
            }
        }

        public static bool GetButton(string buttonName)
        {
            if (IsPassthrough)
            {
                return Input.GetButton(buttonName);
            }
            else
            {
                KeyCode? keyCode = inputManagerSettings.GetPositiveKey(buttonName);
                if (keyCode.HasValue)
                {
                    return keysHeld.Contains(keyCode.Value);
                }
                else
                {
                    Debug.LogWarning("unrecognized button: '" + buttonName + "'");
                    return false;
                }
            }
        }

        public static bool GetButtonDown(string buttonName)
        {
            if (IsPassthrough)
            {
                return Input.GetButtonDown(buttonName);
            }
            else
            {
                KeyCode? keyCode = inputManagerSettings.GetPositiveKey(buttonName);
                if (keyCode.HasValue)
                {
                    return newKeysDown.Contains(keyCode.Value);
                }
                else
                {
                    Debug.LogWarning("unrecognized button: '" + buttonName + "'");
                    return false;
                }
            }
        }

        public static bool GetButtonUp(string buttonName)
        {
            if (IsPassthrough)
            {
                return Input.GetButtonUp(buttonName);
            }
            else
            {
                KeyCode? keyCode = inputManagerSettings.GetPositiveKey(buttonName);
                if (keyCode.HasValue)
                {
                    return newKeysUp.Contains(keyCode.Value);
                }
                else
                {
                    Debug.LogWarning("unrecognized button: '" + buttonName + "'");
                    return false;
                }
            }
        }

        public static float GetAxis(string axisName)
        {
            if (IsPassthrough)
            {
                return Input.GetAxis(axisName);
            }
            else
            {
                KeyCode? posKeyCode = inputManagerSettings.GetPositiveKey(axisName);
                KeyCode? negKeyCode = inputManagerSettings.GetNegativeKey(axisName);
                float value = 0.0f;
                if (posKeyCode.HasValue && keysHeld.Contains(posKeyCode.Value))
                {
                    value += 1.0f;
                }
                if (negKeyCode.HasValue && keysHeld.Contains(negKeyCode.Value))
                {
                    value -= 1.0f;
                }
                return value;
            }
        }

        public static float GetAxisRaw(string axisName)
        {
            if (IsPassthrough)
            {
                return Input.GetAxisRaw(axisName);
            }
            else
            {
                return GetAxis(axisName);
            }
        }
    }
}