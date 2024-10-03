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

    public delegate void GameInputProgressionChanged(GameInputProgression scrambleType);
    public GameInputProgressionChanged OnGameInputProgressionChanged;

    public delegate void MultiplayerScrambleTypeChanged(bool isSameResult);
    public MultiplayerScrambleTypeChanged OnMultiplayerScrambleTypeChanged;

    public delegate void TickDurationChanged(float tickDuration);
    public TickDurationChanged OnTickDurationChanged;

    public delegate void TickEndDurationChanged(float tickEndDuration);
    public TickEndDurationChanged OnTickEndDurationChanged;

    public delegate void MoveOnInputChanged(bool isMoveOnInput);
    public MoveOnInputChanged OnMoveOnInputChanged;

    public delegate void MaxEnergyChanged(int maxEnergy);
    public MaxEnergyChanged OnMaxEnergyChanged;

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
        OnMultiplayerScrambleTypeChanged?.Invoke(gameSettings.isMultiplayerScrambleSame);
        OnTickDurationChanged?.Invoke(gameSettings.tickDuration);
        OnTickEndDurationChanged?.Invoke(gameSettings.tickEndDuration);
        OnMoveOnInputChanged?.Invoke(gameSettings.doesMoveOnInput);
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
            case EffectType.GameInputProgression:                
                var gameInputProgression = (GameInputProgression)Mathf.RoundToInt(effect.amount);
                OnGameInputProgressionChanged?.Invoke(gameInputProgression);
                break;
            case EffectType.TickDuration:
                OnTickDurationChanged?.Invoke(effect.amount); 
                break;
            case EffectType.TickEndDuration:
                OnTickEndDurationChanged?.Invoke(Mathf.RoundToInt(effect.amount));
                break;
            case EffectType.MoveOnInputChanged:
                OnMoveOnInputChanged?.Invoke(Convert.ToBoolean(effect.amount));
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
    ScrambleAmount = 0,
    TickDuration = 1,
    TickEndDuration = 2,
    MoveOnInputChanged = 3,
    DigitalGlitchIntensity = 5,
    ScanLineJitter = 6,
    VerticalJump = 7,
    HorizontalShake = 8,
    ColorDrift = 9,
    MaxEnergyChanged = 10,
    GameInputProgression = 11,
    ScrambleVarience = 12
}