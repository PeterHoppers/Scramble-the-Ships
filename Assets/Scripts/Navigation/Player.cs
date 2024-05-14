using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;
using System.Linq;
using System;

public class Player : Previewable
{
    [SerializedDictionary]
    SerializedDictionary<InputValue, PlayerAction> playerActions = new SerializedDictionary<InputValue, PlayerAction>();

    [SerializeField]
    private ParticleSystem deathVFX;

    GameManager _manager;
    PlayerInput _playerInput;

    public int PlayerId { get; private set; }
    public bool AllowingInput { get; set; }
    InputValue? _lastInput;

    bool _isMatchingDirection;
    bool _isIndistructable = false;

    int _ticksIndistructable = 0;

    [SerializedDictionary]
    public SerializedDictionary<InputValue, SpriteRenderer> inputValueDisplays;

    private void Awake()
    {
        _isMatchingDirection = TestParametersHandler.Instance.testParameters.doesMovementFollowKeys;
        TestParametersHandler.Instance.OnParametersChanged += UpdateScrambleType;
    }

    private void UpdateScrambleType(TestParameters newParameters)
    {
        _isMatchingDirection = newParameters.doesMovementFollowKeys;
    }

    public void InitPlayer(GameManager manager, PlayerInput playerInput, int id)
    {
        _manager = manager;
        _manager.OnTickEnd += OnTickEnd;
        _playerInput = playerInput;
        PlayerId = id;
        AllowingInput = false;
    }

    private void OnTickEnd(float timeToTickStart)
    {
        if (_ticksIndistructable > 0)
        {
            _ticksIndistructable--;

            if (_ticksIndistructable == 0)
            {
                _isIndistructable = false;
            }
        }
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
        if (!AllowingInput)
        {
            return;
        }

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
        if (!AllowingInput)
        {
            return;
        }

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
        if (_lastInput == pressedValue)
        {
            return;
        }

        if (_lastInput != null)
        {
            inputValueDisplays[_lastInput.Value].color = Color.white;
        }

        _lastInput = pressedValue;
        inputValueDisplays[_lastInput.Value].color = Color.grey;

        _manager.AttemptPlayerAction(this, playerAction);
    }

    public override Sprite GetPreviewSprite()
    {
        return GetComponentInChildren<SpriteRenderer>().sprite;
    }

    public void OnHit(Previewable attackingObject)
    {
        if (_isIndistructable)
        {
            attackingObject.DestroyPreviewable();
            return;
        }

        _manager.DestoryPlayer(this, attackingObject);
    }

    public void OnDeath() 
    {
        deathVFX.Play();
        AllowingInput = false;
        SetShipVisiblity(false);
    }

    public void OnSpawn()
    {
        AllowingInput = true;
        _isIndistructable = true;
        _ticksIndistructable = 2;
        SetShipVisiblity(true);
    }

    void SetShipVisiblity(bool isVisible)
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach(var sprite in sprites) 
        { 
            sprite.enabled = isVisible;
        }

        GetComponent<Collider2D>().enabled = isVisible;
    }
}
