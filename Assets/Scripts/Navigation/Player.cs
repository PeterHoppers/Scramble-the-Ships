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
    public ShipInfo shipInfo;
    private ParticleSystem _deathVFX;

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
        _deathVFX = Instantiate(shipInfo.deathVFX, transform);
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

        SendInput(playerMovementInput, playerAction);
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
        SendInput(InputValue.Shoot, playerAction);
    }

    //Takes the input pressed and the action that press triggered
    public void SendInput(InputValue pressedValue, PlayerAction playerAction)
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

        _manager.ClearPreviousPlayerAction(this);
        var targetTile = _manager.GetTileForPlayerAction(playerAction);

        if (targetTile != null && targetTile.IsVisible)
        {
            PreviewAction newPreview;
            Player playerActedUpon = playerAction.playerActionPerformedOn;

            if (playerAction.inputValue == InputValue.Shoot)
            {
                var firingDirection = ConvertInputValueToDirection(playerAction.inputValue);
                newPreview = _manager.CreateMovablePreviewAtTile(playerActedUpon.shipInfo.bullet, playerActedUpon, targetTile, firingDirection);
            }
            else
            {
                newPreview = _manager.CreatePreviewOfPreviewableAtTile(playerActedUpon, targetTile);
            }

            _manager.AddPlayerPreviewAction(this, newPreview);
        }
    }

    public override Sprite GetPreviewSprite()
    {
        return GetComponentInChildren<SpriteRenderer>().sprite;
    }

    public bool CanPlayerDie()
    { 
        return !_isIndistructable;
    }

    public void OnDeath() 
    {
        _deathVFX.Play();
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
