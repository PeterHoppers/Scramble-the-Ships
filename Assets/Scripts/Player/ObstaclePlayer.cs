using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;
using System;

public class ObstaclePlayer : Player
{
    [Space]
    public SerializedDictionary<Screen, List<InputValue>> _playerCommands;
    public PlayerShipInfo playerInfo;

    private ScreenSystem _screenSystem;

    public override void InitPlayer(GameManager manager, PlayerShipInfo shipInfo, int id, InputMoveStyle style)
    {
        base.InitPlayer(manager, shipInfo, id, style);
        _screenSystem = manager.GetComponent<ScreenSystem>();

        //move the rotation from the parent to wherever needs the proper rotation to make the ship fire correctly
        var currentRotation = transform.rotation;        
        transform.rotation = new Quaternion();
        TransitionToRotation(currentRotation, 0);

        SetInputVisibility(false);
    }

    public override List<PlayerAction> GetPossibleActions()
    {
        var currentScreen = _screenSystem.GetCurrentScreen();
        var inputs = _playerCommands[currentScreen];
        var actions = new List<PlayerAction>();

        foreach (var input in inputs) 
        {
            actions.Add(new PlayerAction()
            {
                playerActionPerformedOn = this,
                inputValue = input,
            });        
        }

        return actions;
    }

    public override void SetScrambledActions(SerializedDictionary<ButtonValue, PlayerAction> playerActions)
    {
        if (playerActions.Keys.Count == 0)
        {
            scrambledActions = playerActions;
            SetInputVisibility(false);
        }
        else
        {
            SetInputVisibility(true);
        }

        base.SetScrambledActions(playerActions);
    }

    public override bool HasActiveInput()
    {
        return true;
    }

    public override List<ButtonValue> GetButtonValues(int lastButtonIndex)
    {
        var allButtonValues = (ButtonValue[])Enum.GetValues(typeof(ButtonValue));
        return allButtonValues.ToList().TakeLast(lastButtonIndex).ToList();
    }

    private void OnDestroy()
    {
        _manager.OnPlayerLeaveGame(this);
    }
}
