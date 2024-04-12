using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;
using System.Linq;

public class Player : MonoBehaviour
{
    [SerializedDictionary]
    SerializedDictionary<InputValue, PlayerAction> playerActions = new SerializedDictionary<InputValue, PlayerAction>();
    ControlsManager _manager;
    PlayerInput _playerInput;
    public int PlayerId { get; private set; }
    public bool AllowingInput { get; set; }
    InputValue? _lastInput;

    bool _isMatchingDirection;

    [SerializedDictionary]
    public SerializedDictionary<InputValue, SpriteRenderer> inputValueDisplays;

    private void Awake()
    {
        TestParametersHandler.Instance.OnParametersChanged += UpdateScrambleType;
    }

    private void UpdateScrambleType(TestParameters newParameters)
    {
        _isMatchingDirection = newParameters.doesMovementFollowKeys;
    }

    public void InitPlayer(ControlsManager manager, PlayerInput playerInput, int id)
    {
        _manager = manager;
        _playerInput = playerInput;
        PlayerId = id;
    }

    public void SetPlayerActions(SerializedDictionary<InputValue, PlayerAction> playerActions)
    {
        this.playerActions = playerActions;
        var playerActionKeys = playerActions.Keys;

        foreach (var item in playerActionKeys)
        {
            inputValueDisplays.TryGetValue(item, out var renderer);

            if (renderer != null) 
            {
                renderer.sprite = playerActions[item].actionUI;
            }
        }
    }

    public void ClearSelected()
    {
        if (_lastInput != null)
        {
            inputValueDisplays[_lastInput.Value].color = Color.white;
        }

        _lastInput = null;
    }

    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        Vector2 playerMovement = context.ReadValue<Vector2>();

        if (playerMovement == Vector2.zero)
        {
            return;
        }

        var playerMovementInput = SimplifyDirection(playerMovement);

        PlayerAction playerAction;

        if (_isMatchingDirection)
        {
            playerAction = playerActions[playerMovementInput];
        }
        else
        {
            //find out which element has the value with the input value
            //then return that key
            var buttonAction = playerActions.Values.First(x => x.inputValue == playerMovementInput);
            var valueIndex = playerActions.Values.ToList().IndexOf(buttonAction);
            var targetKey = playerActions.Keys.ToList()[valueIndex];
            playerAction = playerActions.Values.First(x => x.inputValue == targetKey);
            playerMovementInput = targetKey;
        }

        SendInput(playerAction, playerMovementInput);
    }

    InputValue SimplifyDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                return InputValue.Right;
            }
            else
            {
                return InputValue.Left;
            }
        }
        else
        {
            if (direction.y > 0)
            {
                return InputValue.Up;
            }
            else
            {
                return InputValue.Down;
            }
        }
    }

    public void OnPlayerFire(InputAction.CallbackContext context)
    {
        var fired = context.ReadValueAsButton();

        if (fired == false)
        {
            return;
        }

        var playerAction = playerActions[InputValue.Shoot];
        SendInput(playerAction, InputValue.Shoot);
    }

    public void SendInput(PlayerAction playerAction, InputValue pressedValue)
    {
        if (!AllowingInput)
        {
            return;
        }

        if (_lastInput == pressedValue)
        {
            return;
        }

        if (_lastInput != null)
        {
            inputValueDisplays[_lastInput.Value].color = Color.white;
        }

        _lastInput = pressedValue;
        inputValueDisplays[_lastInput.Value].color = Color.black;

        _manager.SendInput(this, playerAction);
    }
}
