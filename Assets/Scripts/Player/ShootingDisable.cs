using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingDisable : Condition
{
    protected InputValue _disabledValue = InputValue.Fire;

    public override void OnConditionStart(Player player, int duration)
    {
        base.OnConditionStart(player, duration);
        player.RemovePossibleInput(_disabledValue);
    }

    public override void OnTickEnd()
    {
        //do nothing since this can't end up normal means
    }

    public override void RemoveCondition()
    {
        Player.AddPossibleInput(_disabledValue);
        base.RemoveCondition();
    }
}
