using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using URPGlitch.Runtime.AnalogGlitch;
using URPGlitch.Runtime.DigitalGlitch;
using UnityEngine.Rendering;

public class EffectsSystem : MonoBehaviour
{
    public delegate void ScrambleAmountChanged(int scrambleAmount);
    public ScrambleAmountChanged OnScrambleAmountChanged;

    public delegate void ScrambleVarianceChanged(int scrambleVariance);
    public ScrambleVarianceChanged OnScrambleVarianceChanged;

    public delegate void ScrambleTypeChanged(ScrambleType scrambleType);
    public ScrambleTypeChanged OnScrambleTypeChanged;

    public delegate void MultiplayerScrambleTypeChanged(bool isSameResult);
    public MultiplayerScrambleTypeChanged OnMultiplayerScrambleTypeChanged;

    public delegate void TickDurationChanged(float tickDuration);
    public TickDurationChanged OnTickDurationChanged;

    public delegate void TicksUntilScrambleChanged(int tickAmount);
    public TicksUntilScrambleChanged OnTicksUntilScrambleChanged;

    public delegate void MoveOnInputChanged(bool isMoveOnInput);
    public MoveOnInputChanged OnMoveOnInputChanged;

    public delegate void MaxEnergyChanged(int maxEnergy);
    public MaxEnergyChanged OnMaxEnergyChanged;

    public delegate void ConditionChanged<T>(bool isGained) where T : Condition;
    public ConditionChanged<ShootingDisable> OnShootingChanged;

    //Visual Effects
    [SerializeField]
    Volume _mainCameraVolume;
    private DigitalGlitchVolume _digitalGlitchVolume;
    private AnalogGlitchVolume _analogueGlichVolume;

    private void Awake()
    {
        _mainCameraVolume.profile.TryGet<DigitalGlitchVolume>(out var digitalGlitchVolume);
        _digitalGlitchVolume = digitalGlitchVolume;
        _mainCameraVolume.profile.TryGet<AnalogGlitchVolume>(out var analogGlitchVolume);
        _analogueGlichVolume = analogGlitchVolume;
    }

    private void Start()
    {
        OptionsManager.Instance.OnParametersChanged += InvokeCurrentParameters;
    }

    private void InvokeCurrentParameters(GameSettingParameters gameSettings, SystemSettingParameters systemSettingParameters)
    {
        OnScrambleAmountChanged?.Invoke(gameSettings.amountControlsScrambled);
        OnScrambleTypeChanged?.Invoke(gameSettings.scrambleType);
        OnMultiplayerScrambleTypeChanged?.Invoke(gameSettings.isMultiplayerScrambleSame);
        OnTickDurationChanged?.Invoke(gameSettings.tickDuration);
        OnTicksUntilScrambleChanged?.Invoke(gameSettings.amountTickPerScramble);
        OnMoveOnInputChanged?.Invoke(gameSettings.doesMoveOnInput);
        OnShootingChanged?.Invoke(gameSettings.isShootingEnabled);
        OnMaxEnergyChanged?.Invoke(gameSettings.maxEnergy);
    }

    public void PerformEffect(EffectType effectType, float effectAmount)
    {
        PerformEffect(new Effect()
        {
            type = effectType,
            amount = effectAmount
        });
    }

    public void PerformEffect(Effect effect)
    { 
        switch(effect.type) 
        { 
            case EffectType.ScrambleAmount:
                OnScrambleAmountChanged?.Invoke(Mathf.RoundToInt(effect.amount));
                break;
            case EffectType.ScrambleVarience:
                OnScrambleVarianceChanged?.Invoke(Mathf.RoundToInt(effect.amount));
                break;
            case EffectType.ScrambleType:
                OnScrambleTypeChanged?.Invoke((ScrambleType)Mathf.RoundToInt(effect.amount));
                break;
            case EffectType.TickDuration:
                OnTickDurationChanged?.Invoke(effect.amount); 
                break;
            case EffectType.TicksUntilScramble:
                OnTicksUntilScrambleChanged?.Invoke(Mathf.RoundToInt(effect.amount));
                break;
            case EffectType.MoveOnInputChanged:
                OnMoveOnInputChanged?.Invoke(Convert.ToBoolean(effect.amount));
                break;
            case EffectType.ShootingChanged:
                OnShootingChanged?.Invoke(Convert.ToBoolean(effect.amount));
                break;
            case EffectType.MaxEnergyChanged:
                OnMaxEnergyChanged?.Invoke(Mathf.RoundToInt(effect.amount));
                break;
            case EffectType.DigitalGlitchIntensity:
                _digitalGlitchVolume.intensity.value = effect.amount;
                break;
            case EffectType.ScanLineJitter:
                _analogueGlichVolume.scanLineJitter.value = effect.amount;
                break;
            case EffectType.VerticalJump:
                _analogueGlichVolume.verticalJump.value = effect.amount;
                break;
            case EffectType.HorizontalShake:
                _analogueGlichVolume.horizontalShake.value = effect.amount;
                break;
            case EffectType.ColorDrift:
                _analogueGlichVolume.colorDrift.value = effect.amount;
                break;
        }
    }

    public void ClearCameraEffects()
    {
        _digitalGlitchVolume.intensity.value = 0;
        _analogueGlichVolume.scanLineJitter.value = 0;
        _analogueGlichVolume.verticalJump.value = 0;
        _analogueGlichVolume.horizontalShake.value = 0;
        _analogueGlichVolume.colorDrift.value = 0;
    }
}

[System.Serializable]
public struct Effect
{ 
    public EffectType type;
    public float amount;
}

public enum EffectType
{ 
    ScrambleAmount,
    TickDuration,
    TicksUntilScramble,
    MoveOnInputChanged,
    ShootingChanged,
    DigitalGlitchIntensity,
    ScanLineJitter,
    VerticalJump,
    HorizontalShake,
    ColorDrift,
    MaxEnergyChanged,
    ScrambleType,
    ScrambleVarience
}