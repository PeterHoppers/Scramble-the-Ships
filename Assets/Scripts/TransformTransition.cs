using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTransition : Transitioner
{
    public AnimationCurve positionCurve;
    public AnimationCurve rotationCurve;
    public AnimationCurve scaleCurve;

    public void MoveTo(Vector3 target, float duration, bool isLocalPosition = true)
    {
        Vector3 origin = (isLocalPosition) ? transform.localPosition : transform.position;
        StartCoroutine(AnimateMovement(origin, target, duration, isLocalPosition));
    }

    IEnumerator AnimateMovement(Vector3 origin, Vector3 target, float duration, bool isLocalPosition)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey += Time.deltaTime;
            float curvePercent = GetCurvePercent(journey, duration, positionCurve);

            if (isLocalPosition)
            {
                transform.localPosition = Vector3.LerpUnclamped(origin, target, curvePercent);
            }
            else
            {
                transform.position = Vector3.LerpUnclamped(origin, target, curvePercent);
            }

            yield return null;
        }
    }

    public void ScaleTo(Vector3 targetSize, float duration, Transform transformToScale = null)
    {
        if (transformToScale == null)
        {
            if (transform == null)
            {
                return;
            }

            transformToScale = transform;
        }


        StartCoroutine(AnimateScale(transformToScale, transformToScale.localScale, targetSize, duration));
    }

    IEnumerator AnimateScale(Transform targetTransform, Vector3 origin, Vector3 target, float duration)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey += Time.deltaTime;
            float curvePercent = GetCurvePercent(journey, duration, scaleCurve);

            targetTransform.localScale = Vector3.LerpUnclamped(origin, target, curvePercent);

            yield return null;
        }

        targetTransform.localScale = target;
    }

    public void RotateTo(Quaternion targetRotation, float duration, Transform transformToRotate = null, bool isLocal = false)
    {
        if (transformToRotate == null)
        {
            transformToRotate = transform;
        }
        if (duration == 0)
        {
            transformToRotate.rotation = targetRotation;
        }

        StartCoroutine(AnimateRotation(transformToRotate, targetRotation, duration, isLocal));
    }

    IEnumerator AnimateRotation(Transform rotatingTransform, Quaternion target, float duration, bool isLocal)
    {
        float journey = 0f;
        Quaternion origin = rotatingTransform.rotation;
        while (journey <= duration)
        {
            journey += Time.deltaTime;
            float curvePercent = GetCurvePercent(journey, duration, rotationCurve);

            if (isLocal)
            {
                rotatingTransform.localRotation = Quaternion.SlerpUnclamped(origin, target, curvePercent);
            }
            else
            {
                rotatingTransform.rotation = Quaternion.SlerpUnclamped(origin, target, curvePercent);
            }

            yield return null;
        }
    }    
}