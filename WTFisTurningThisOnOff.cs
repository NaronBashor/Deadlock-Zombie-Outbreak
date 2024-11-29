using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WTFisTurningThisOnOff : MonoBehaviour
{
    private void OnEnable()
    {
        //Debug.Log("Dialogue Panel Activated by: " + GetTriggeringObject());
    }

    private void OnDisable()
    {
        //Debug.Log("Dialogue Panel Deactivated by: " + GetTriggeringObject());
    }

    private string GetTriggeringObject()
    {
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

        // Go through the stack trace and find the first method outside of this class
        for (int i = 1; i < stackTrace.FrameCount; i++) {
            var method = stackTrace.GetFrame(i).GetMethod();
            if (method.DeclaringType != typeof(WTFisTurningThisOnOff)) {
                return $"{method.DeclaringType}.{method.Name}";
            }
        }
        return "Unknown";
    }
}
