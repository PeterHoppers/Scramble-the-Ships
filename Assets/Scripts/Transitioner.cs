using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transitioner : MonoBehaviour
{
    protected float GetCurvePercent(float journey, float duration, AnimationCurve curve)
    {
        float percent = Mathf.Clamp01(journey / duration);
        return curve.Evaluate(percent);
    }
}
