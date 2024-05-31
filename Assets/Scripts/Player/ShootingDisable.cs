using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingDisable : Condition
{
    protected InputValue _disabledValue = InputValue.Fire;
    void Start()
    {
        Duration = int.MaxValue;
    }

    public override void OnConditionStart(Player player)
    {
        base.OnConditionStart(player);
        player.RemovePossibleInput(_disabledValue);
    }

    public override void OnTickEnd()
    {
        //do nothing since this can't end up normal means
    }

    protected override void RemoveCondition()
    {
        Player.AddPossibleInput(_disabledValue);
        base.RemoveCondition();
    }
}
