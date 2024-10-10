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

    public delegate void InputMoveStyleChanged(InputMoveStyle moveStyle);
    public InputMoveStyleChanged OnInputMoveStyleChanged;

    public delegate void MaxEnergyChanged(int maxEnergy);
    public MaxEnergyChanged OnMaxEnergyChanged;

    private GlitchAdapter _glitchAdapter;

    private void Awake()
    {
        _glitchAdapter = Camera.main.GetComponent<GlitchAdapter>();
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
        OnInputMoveStyleChanged?.Invoke(gameSettings.inputMoveStyle);
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
            case EffectType.MaxEnergyChanged:
                OnMaxEnergyChanged?.Invoke(Mathf.RoundToInt(effect.amount));
                break;
            case EffectType.DigitalGlitchIntensity:
                _glitchAdapter.SetDigitalGlitchIntensity(effect.amount);
                break;
            case EffectType.ScanLineJitter:
                _glitchAdapter.SetScanLineJitterIntensity(effect.amount);
                break;
            case EffectType.VerticalJump:
                _glitchAdapter.SetVerticalJumpIntensity(effect.amount);
                break;
            case EffectType.HorizontalShake:
                _glitchAdapter.SetHorizontalShakeIntensity(effect.amount);
                break;
            case EffectType.ColorDrift:
                _glitchAdapter.SetColorDriftIntensity(effect.amount);
                break;
        }
    }

    public void ClearCameraEffects()
    {
        _glitchAdapter.ClearGlitchEffects();
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
    DigitalGlitchIntensity = 5,
    ScanLineJitter = 6,
    VerticalJump = 7,
    HorizontalShake = 8,
    ColorDrift = 9,
    MaxEnergyChanged = 10,
    GameInputProgression = 11,
    ScrambleVarience = 12
}