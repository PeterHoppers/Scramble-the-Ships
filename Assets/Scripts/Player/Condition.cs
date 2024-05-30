using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Condition : MonoBehaviour
{
    protected int Duration { get; set; }
    protected Player Player { get; set; }
    private int _ticksRemaining;

    public virtual void OnConditionStart(Player player)
    {
        Player = player;
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

    protected virtual void RemoveCondition()
    {
        Player.RemoveCondition(this);
        Destroy(this);
    }
}
