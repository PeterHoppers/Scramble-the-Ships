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
    public SerializedDictionary<Screen, GridObjectCommands> _defaultShipCommands;
    public PlayerShipInfo playerInfo;

    private int _tickIndex = 0;
    private SerializedDictionary<int, InputValue> _shipCommands;

    private InputValue _currentInputValue = InputValue.None;
    private List<PlayerAction> _possibleShipActions = new List<PlayerAction>();


    public override void InitPlayer(GameManager manager, PlayerShipInfo shipInfo, int id, InputMoveStyle style)
    {
        base.InitPlayer(manager, shipInfo, id, style);
        manager.OnTickStart += CreateNextPreview;
        var screen = _manager.GetComponent<ScreenSystem>().GetCurrentScreen();
        SetupConfiguration(screen);

        //move the rotation from the parent to wherever needs the proper rotation to make the ship fire correctly
        var currentRotation = transform.rotation;        
        transform.rotation = new Quaternion();
        TransitionToRotation(currentRotation, 0);

        SetInputVisibility(false);
    }

    void SetupConfiguration(Screen screen)
    {
        if (_defaultShipCommands.TryGetValue(screen, out var commands))
        {
            _shipCommands = commands.commands;
        }

        if (!_playerCommands.TryGetValue(screen, out var playerInputOptions))
        {
            return;
        }

        var actions = new List<PlayerAction>();

        foreach (var input in playerInputOptions)
        {
            actions.Add(new PlayerAction()
            {
                playerActionPerformedOn = this,
                inputValue = input,
            });
        }

        _possibleShipActions = actions;

        OnPossibleInputsChanged?.Invoke(_possibleShipActions);
    }

    public override List<PlayerAction> GetPossibleActions()
    {
        return _possibleShipActions;
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

    protected void CreateNextPreview(float _)
    {
        if (_shipCommands != null && _shipCommands.TryGetValue(_tickIndex, out var inputValue))
        {
            _currentInputValue = inputValue;
        }

        SendPlayerAction(new PlayerAction()
        {
            playerActionPerformedOn = this,
            inputValue = _currentInputValue
        });
    }

    protected override void OnTickEnd(float tickEndDuration, int nextTickNumber)
    {
        base.OnTickEnd(tickEndDuration, nextTickNumber);
        _tickIndex = nextTickNumber;
    }

    private void OnDestroy()
    {
        _manager.OnTickStart -= CreateNextPreview;
        StopAllCoroutines();
        _manager.OnPlayerLeaveGame(this);
    }
}
