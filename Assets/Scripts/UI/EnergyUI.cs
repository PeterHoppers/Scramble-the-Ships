using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private Image _energyFrontBar;
    [SerializeField]
    private Image _energyBackBar;

    [Header("Animation Settings")]
    [SerializeField]
    private AnimationCurve _fillCurve;
    [SerializeField]
    private float fillDuration;

    [SerializeField]
    private Color lossEnergyColor;
    [SerializeField]
    private Color gainEnergyColor;

    private int _pastEnergy;
    private int _currentEnergy;

    // Start is called before the first frame update
    void Start()
    {
        _energyFrontBar.fillAmount = 1f;
        _energyBackBar.fillAmount = 1f;
    }

    public void SetEnergy(int currentEnergy, int maxEnergy)
    { 
        _pastEnergy = _currentEnergy;
        _currentEnergy = currentEnergy;
        float currentPercentageRemaining = _currentEnergy / (float)maxEnergy;
        float previousPercentageRemaining = _pastEnergy / (float)maxEnergy;

        var frontFill = _energyFrontBar.fillAmount;
        var backFill = _energyBackBar.fillAmount;

        if (previousPercentageRemaining > currentPercentageRemaining)
        {
            _energyFrontBar.fillAmount = currentPercentageRemaining;
            _energyBackBar.color = lossEnergyColor;
            StopAllCoroutines();
            StartCoroutine(AnimateFill(_energyBackBar, backFill, currentPercentageRemaining, fillDuration));
        }
        else
        {
            _energyBackBar.color = gainEnergyColor;
            _energyBackBar.fillAmount = currentPercentageRemaining;
            StopAllCoroutines();
            StartCoroutine(AnimateFill(_energyFrontBar, frontFill, currentPercentageRemaining, fillDuration));
        }
    }

    IEnumerator AnimateFill(Image imageFilling, float startingValue, float endingValue, float duration)
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

    float GetCurvePercent(float journey, float duration, AnimationCurve curve)
    {
        float percent = Mathf.Clamp01(journey / duration);
        return curve.Evaluate(percent);
    }
}
