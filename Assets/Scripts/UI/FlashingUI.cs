using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashingUI : MonoBehaviour
{
    private Material _flashingMaterial;
    
    [SerializeField]
    private AnimationCurve _fillCurve;
    [SerializeField]
    private float _minIntensity;
    [SerializeField] 
    private float _maxIntensity;
    [SerializeField]
    private float _flashDuration;

    // Start is called before the first frame update
    void Start()
    {
        var baseImage = GetComponent<Image>();
        var baseMaterial = baseImage.material;
        _flashingMaterial = Instantiate(baseMaterial);
        baseImage.material = _flashingMaterial;
    }

    public void StartFlashing()
    { 
        var targetColor = (Color)_flashingMaterial.GetVector("_GlowColor");
        StartCoroutine(LerpBetweenIntensity(targetColor, _minIntensity, _maxIntensity, _flashDuration));
    }

    public void StopFlashing()
    { 
        StopAllCoroutines();
    }

    IEnumerator LerpBetweenIntensity(Color baseColor, float startingValue, float endingValue, float duration)
    {
        float fillProgress = 0f;
        while (fillProgress <= duration)
        {
            fillProgress += Time.deltaTime;
            float curvePercent = GetCurvePercent(fillProgress, duration, _fillCurve);

            var targetIntensity = Mathf.LerpUnclamped(startingValue, endingValue, curvePercent);
            _flashingMaterial.SetColor("_GlowColor", baseColor * targetIntensity);

            yield return null;
        }

        StartCoroutine(LerpBetweenIntensity(baseColor, endingValue, startingValue, duration));
    }

    float GetCurvePercent(float journey, float duration, AnimationCurve curve)
    {
        float percent = Mathf.Clamp01(journey / duration);
        return curve.Evaluate(percent);
    }
}
