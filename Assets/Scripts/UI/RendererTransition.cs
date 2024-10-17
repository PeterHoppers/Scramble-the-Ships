using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RendererTransition : Transitioner
{    
    [SerializeField]
    private AnimationCurve _fillCurve;

    public void AnimateFill(Image imageFilling, float startingValue, float endingValue, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FillImage(imageFilling, startingValue, endingValue, duration));
    }

    IEnumerator FillImage(Image imageFilling, float startingValue, float endingValue, float duration)
    {
        float fillProgress = 0f;
        while (fillProgress <= duration)
        {
            fillProgress += Time.deltaTime;
            float curvePercent = GetCurvePercent(fillProgress, duration, _fillCurve);

            imageFilling.fillAmount = Mathf.LerpUnclamped(startingValue, endingValue, curvePercent);

            yield return null;
        }
    }
}
