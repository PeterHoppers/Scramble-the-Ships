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

public class RotatingCondition : Condition
{ 
    protected List<InputValue> _valuesToRemove = new List<InputValue>() 
    { 
        InputValue.Port,
        InputValue.Starboard
    };

    protected List<InputValue> _valuesToAdd = new List<InputValue>()
    {
        InputValue.Clockwise,
        InputValue.Counterclockwise
    };

    public override void OnConditionStart(Player player, int duration)
    {
        base.OnConditionStart(player, duration);
        player.RemovePossibleInput(InputValue.Fire); //to move it to the end of the list to allow us to easily not scramble shooting
        
        foreach (InputValue value in _valuesToRemove) 
        { 
            player.RemovePossibleInput(value);
        }

        foreach (InputValue value in _valuesToAdd)
        { 
            player.AddPossibleInput(value);
        }

        player.AddPossibleInput(InputValue.Fire); //to move it to the end of the list to allow us to easily not scramble shooting
    }

    public override void OnTickEnd()
    {
        //do nothing since this can't end up normal means
    }

    public override void RemoveCondition()
    {
        Player.RemovePossibleInput(InputValue.Fire); //to move it to the end of the list to allow us to easily not scramble shooting

        foreach (InputValue value in _valuesToAdd)
        {
            Player.RemovePossibleInput(value);
        }

        foreach (InputValue value in _valuesToRemove)
        {
            Player.AddPossibleInput(value);
        }

        Player.AddPossibleInput(InputValue.Fire); //to move it to the end of the list to allow us to easily not scramble shooting

        base.RemoveCondition();
    }
}