using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RendererTransition))]
public class EnergyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private Image _energyFrontBar;
    [SerializeField]
    private Image _energyBackBar;

    [Header("Animation Settings")]
    [SerializeField]
    private float _energyLossDuration;
    [SerializeField]
    private float _energyGainedDuration;
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
    private RendererTransition _rendererTransition;

    // Start is called before the first frame update
    void Start()
    {
        _energyFrontBar.fillAmount = 1f;
        _defaultColor = _energyFrontBar.color;
        _flashingUI = _energyFrontBar.GetComponent<FlashingUI>();
        _rendererTransition = GetComponent<RendererTransition>();
        _energyBackBar.fillAmount = 1f;
    }

    public void SetEnergy(int currentEnergy, int maxEnergy)
    {
        if (!isActiveAndEnabled)
        {
            return;
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
            _rendererTransition.AnimateFill(_energyBackBar, backFill, currentPercentageRemaining, _energyLossDuration);
        }
        else
        {
            _energyBackBar.color = gainEnergyColor;
            _energyBackBar.fillAmount = currentPercentageRemaining;
            _rendererTransition.AnimateFill(_energyFrontBar, frontFill, currentPercentageRemaining, _energyGainedDuration);
        }        
    }
}
