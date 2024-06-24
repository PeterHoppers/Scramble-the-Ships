using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Condition : MonoBehaviour
{
    protected int Duration { get; set; }
    protected Player Player { get; set; }
    private int _ticksRemaining;

    public virtual void OnConditionStart(Player player, int duration)
    {
        Player = player;
        Duration = duration;
        _ticksRemaining = Duration;
    }

    public virtual void OnTickEnd()
    {
        _ticksRemaining--;

        if (_ticksRemaining <= 0)
        { 
            RemoveCondition();
        }
    }

    public virtual bool OnPlayerHit() 
    {
        return true;
    }

    public virtual void OnPlayerDeath()
    {
        RemoveCondition();
    }

    public virtual void RemoveCondition() //turn this back into protected if we remove UI to toggle this on and off
    {
        Player.RemoveCondition(this);        
    }
}
