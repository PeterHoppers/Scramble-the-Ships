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
    [Range(0, 1)]
    private float lowPercentage;

    [SerializeField]
    private Color lossEnergyColor;
    [SerializeField]
    private Color gainEnergyColor;
    [SerializeField]
    private Color lowEnergyColor;
    [SerializeField]
    private Color lossLowEnergyColor;

    private int _pastEnergy;
    private int _currentEnergy;
    private Color _defaultColor;
    private bool _isLowEnergy = false;
    private FlashingUI _flashingUI;

    // Start is called before the first frame update
    void Start()
    {
        _energyFrontBar.fillAmount = 1f;
        _defaultColor = _energyFrontBar.color;
        _flashingUI = _energyFrontBar.GetComponent<FlashingUI>();
        _energyBackBar.fillAmount = 1f;
        gameObject.SetActive(false);
    }

    public void SetEnergy(int currentEnergy, int maxEnergy)
    {
        if (_pastEnergy == 0)
        { 
            gameObject.SetActive(true);
        }

        _pastEnergy = _currentEnergy;
        _currentEnergy = currentEnergy;
        float currentPercentageRemaining = _currentEnergy / (float)maxEnergy;
        float previousPercentageRemaining = _pastEnergy / (float)maxEnergy;

        var frontFill = _energyFrontBar.fillAmount;
        var backFill = _energyBackBar.fillAmount;

        if (currentPercentageRemaining < lowPercentage && !_isLowEnergy)
        {
            _isLowEnergy = true;
            _energyFrontBar.color = lowEnergyColor;
            _flashingUI.StartFlashing();
        }
        else if (currentPercentageRemaining >= lowPercentage && _isLowEnergy)
        {
            _isLowEnergy = false;
            _energyFrontBar.color = _defaultColor;
            _flashingUI.StopFlashing();
        }

        if (previousPercentageRemaining > currentPercentageRemaining)
        {
            _energyFrontBar.fillAmount = currentPercentageRemaining;
            _energyBackBar.color = (_isLowEnergy) ? lossLowEnergyColor : lossEnergyColor;
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

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
