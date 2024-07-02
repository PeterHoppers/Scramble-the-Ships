using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
