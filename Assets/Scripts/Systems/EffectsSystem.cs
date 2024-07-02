using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectsSystem : MonoBehaviour
{
    public delegate void ScrambleAmountChanged(int scrambleAmount);
    public ScrambleAmountChanged OnScrambleAmountChanged;

    public delegate void TickDurationChanged(float tickDuration);
    public TickDurationChanged OnTickDurationChanged;

    public delegate void TicksUntilScrambleChanged(int tickAmount);
    public TicksUntilScrambleChanged OnTicksUntilScrambleChanged;

    public delegate void MoveOnInputChanged(bool isMoveOnInput);
    public MoveOnInputChanged OnMoveOnInputChanged;

    public delegate void ConditionChanged<T>(bool isGained) where T : Condition;
    public ConditionChanged<ShootingDisable> OnShootingChanged;

    public void PerformEffect(Effect effect)
    { 
        switch(effect.type) 
        { 
            case EffectType.ScrambleAmount:
                OnScrambleAmountChanged?.Invoke(Mathf.RoundToInt(effect.amount));
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
        }
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
    ShootingChanged
}
