using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;
using System.Linq;
using System;
using UnityEngine.Windows;

public class Player : Previewable
{
    [SerializedDictionary]
    SerializedDictionary<InputValue, PlayerAction> scrambledActions = new SerializedDictionary<InputValue, PlayerAction>();
    
    private ShipInfo _shipInfo;
    private ParticleSystem _deathVFX;

    PlayerInput _playerInput;
    Sprite _shipSprite;
    Collider2D _shipCollider;

    public int PlayerId { get; private set; }
    public bool AllowingInput { get; set; }
    private bool _isDestroyed = false;
    InputValue? _lastInput;

    List<PlayerAction> _possibleActions = new List<PlayerAction>();
    List<Condition> _playerConditions = new List<Condition>();

    [SerializedDictionary]
    public SerializedDictionary<InputValue, InputRenderer> inputValueDisplays;

    private void Awake()
    {
        TestParametersHandler.Instance.OnParametersChanged += ChangeShootingCondition;
        _shipCollider = GetComponent<Collider2D>();
    }

    private void ChangeShootingCondition(TestParameters newParameters)
    {
        bool isShootingDisabled = _playerConditions.Any(x => x.GetType() == typeof(ShootingDisable));

        if (isShootingDisabled) 
        {
            if (newParameters.isShootingEnabled)
            {
                var condition = _playerConditions.First(x => x.GetType() == typeof(ShootingDisable));
                condition.RemoveCondition(); //kind of going through the backdoor here. The condition should normally end itself, not some UI
                RemoveCondition(condition);
            }
        }
        
        if (!newParameters.isShootingEnabled) 
        {
            AddCondition<ShootingDisable>(int.MaxValue);
        }
    }

    private void Start()
    {
        //for now, let's just add removing shooting on start. Let's consider adding on conditional check to see if we should
        ChangeShootingCondition(TestParametersHandler.Instance.testParameters);
    }

    public void InitPlayer(GameManager manager, PlayerInput playerInput, ShipInfo shipInfo, int id)
    {
        _manager = manager;
        _manager.OnTickStart += OnTickStart;
        _manager.OnTickEnd += OnTickEnd;
        _playerInput = playerInput;
        _shipInfo = shipInfo;
        PlayerId = id;
        AllowingInput = false;

        foreach (InputValue inputValue in Enum.GetValues(typeof(InputValue)))
        {
            AddPossibleInput(inputValue);
        }

        _deathVFX = Instantiate(_shipInfo.deathVFX, transform);
        _shipSprite = _shipInfo.shipSprite;
        GetComponentInChildren<SpriteRenderer>().sprite = _shipSprite;        
    }

    private void OnTickStart(float _)
    {
        if (_isDestroyed)
        {
            return;
        }

        AllowingInput = true;
        ClearSelected();
    }

    private void OnTickEnd(int _)
    {
        if (_isDestroyed) 
        {
            return;
        }

        AllowingInput = false;

        foreach (var inputValue in inputValueDisplays)
        {
            if (_lastInput != null && _lastInput == inputValue.Key)
            {
                continue;
            }

            inputValue.Value.SetVisibility(false);
        }

        //looping backwards like this allows us to safely remove items from the list
        for (int i = _playerConditions.Count - 1; i >= 0; i--) 
        {
            var condition = _playerConditions[i];
            condition.OnTickEnd();
        }       
    }

    public List<PlayerAction> GetPossibleAction()
    { 
        return _possibleActions;
    }

    public void AddPossibleInput(InputValue inputToRemove)
    {
        if (_possibleActions.Count(x => x.inputValue == inputToRemove) > 0)
        {
            //return;
        }

        _possibleActions.Add(new PlayerAction()
        {
            playerActionPerformedOn = this,
            inputValue = inputToRemove,
            actionUI = _shipInfo.inputsForSprites[inputToRemove]
        });
    }

    public void RemovePossibleInput(InputValue inputToRemove)
    {
        _possibleActions.RemoveAll(x => x.inputValue == inputToRemove);
    }

    public void SetScrambledActions(SerializedDictionary<InputValue, PlayerAction> playerActions)
    {
        scrambledActions = playerActions;
        var playerActionKeys = playerActions.Keys;

        foreach (var item in playerActionKeys)
        {
            inputValueDisplays.TryGetValue(item, out var renderer);

            if (renderer != null) 
            {
                renderer.SetSprite(playerActions[item].actionUI);
            }
        }
    }

    public void ClearSelected()
    {
        foreach (var inputValue in inputValueDisplays)
        {
            inputValue.Value.SetVisibility(true);
        }

        if (_lastInput != null)
        {
            inputValueDisplays[_lastInput.Value].DeselectInput();
        }

        _lastInput = null;
    }

    public void SetInputStatus(bool isActive)
    {
        AllowingInput = isActive;
        SetInputVisibility(isActive);
        _shipCollider.enabled = isActive;
    }

    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        if (!AllowingInput || _isDestroyed)
        {
            return;
        }

        Vector2 playerMovement = context.ReadValue<Vector2>();

        if (playerMovement == Vector2.zero)
        {
            return;
        }

        var playerMovementInput = SimplifyDirection(playerMovement);

        if(scrambledActions.TryGetValue(playerMovementInput, out var playerAction))
        {
            SendInput(playerMovementInput, playerAction);
        }
    }

    InputValue SimplifyDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                return InputValue.Starboard;
            }
            else
            {
                return InputValue.Port;
            }
        }
        else
        {
            if (direction.y > 0)
            {
                return InputValue.Forward;
            }
            else
            {
                return InputValue.Backward;
            }
        }
    }

    public void OnPlayerFire(InputAction.CallbackContext context)
    {
        if (!AllowingInput || _isDestroyed)
        {
            return;
        }

        var fired = context.ReadValueAsButton();

        if (fired == false)
        {
            return;
        }

        if (scrambledActions.TryGetValue(InputValue.Fire, out var playerAction))
        {
            SendInput(InputValue.Fire, playerAction);
        }
    }

    //unsure if this should stay here, since there's a good chance that this might need to be brought up outside of normal play when we move to production
    public void OnPlayerPause(InputAction.CallbackContext context)
    {
        _manager.PauseGame();
    }

    public void OnPlayerRestart(InputAction.CallbackContext context)
    {
        _manager.RestartGame();
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
            inputValueDisplays[_lastInput.Value].DeselectInput();
        }

        _lastInput = pressedValue;
        inputValueDisplays[_lastInput.Value].SelectInput();

        _manager.ClearPreviousPlayerAction(this);
        var targetTile = _manager.GetTileForPlayerAction(playerAction);

        if (targetTile != null && targetTile.IsVisible)
        {
            PreviewAction newPreview;
            Player playerActedUpon = playerAction.playerActionPerformedOn;

            if (playerAction.inputValue == InputValue.Fire)
            {
                var firingDirection = ConvertInputValueToDirection(playerAction.inputValue);
                var bullet = _manager.CreateMovableAtTile(playerActedUpon._shipInfo.bullet, playerActedUpon, targetTile, firingDirection);
                newPreview = _manager.CreatePreviewOfPreviewableAtTile(bullet, targetTile);
                newPreview.isCreated = true;
                _manager.AddPreviewAction(newPreview);
            }
            else
            {
                newPreview = _manager.CreatePreviewOfPreviewableAtTile(playerActedUpon, targetTile);
            }

            _manager.AddPlayerPreviewAction(this, newPreview);
        }
    }

    public bool OnHit()
    {
        if (_isDestroyed)
        { 
            return false;
        }

        bool isPlayerDead = true;
        _playerConditions.ForEach(condition =>
        {
            isPlayerDead = condition.OnPlayerHit();
            if (!isPlayerDead) 
            {
                return;
            }
        });

        return isPlayerDead;
    }

    public void OnDeath() 
    {
        _deathVFX.Play();
        _isDestroyed = true;
        SetShipVisiblity(false);
    }

    public void OnSpawn()
    {
        _isDestroyed = false;
        AddCondition<Respawn>(Respawn.RespawnDuration);
        _manager.OnTickStart += ShowVisiblity;

        //use a local function to queue up a function for the next call of a event
        void ShowVisiblity(float _)
        {
            SetShipVisiblity(true);
            _manager.OnTickStart -= ShowVisiblity;
        }
    }

    void SetShipVisiblity(bool isVisible)
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();

        foreach(var sprite in sprites) 
        { 
            sprite.enabled = isVisible;
        }

        if (_shipCollider == null)
        {
            _shipCollider = GetComponent<Collider2D>();
        }
        _shipCollider.enabled = isVisible;
    }

    void SetInputVisibility(bool isVisible)
    {
        foreach (var item in inputValueDisplays)
        {
            item.Value.SetVisibility(isVisible);
        }
    }

    public void AddCondition<T>(int duration) where T : Condition
    {
        var newCondition = gameObject.AddComponent<T>();
        newCondition.OnConditionStart(this, duration);
        _playerConditions.Add(newCondition);
        _manager.PlayerGainedCondition(this, newCondition);
    }

    public void RemoveCondition(Condition condition)
    { 
        _playerConditions.Remove(condition);
        _manager.PlayerLostCondition(this, condition);
        Destroy(condition); //TODO: Consider pooling/disabling rather than creating and destorying
    }

    public override Sprite GetPreviewSprite()
    {
        return _shipSprite;
    }

    public Sprite GetSpriteForInput(InputValue input)
    {
        if (_shipInfo.inputsForSprites.TryGetValue(input, out var sprite))
        {
            return sprite;
        }

        return null;
    }

    public void AddInputRenderer(InputValue value, InputRenderer renderer)
    {
        inputValueDisplays.Add(value, renderer);
    }

    protected override void PerformInteraction(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out var player))
        {
            _manager.HandlePlayerCollision(this, player);
        }

        base.PerformInteraction(collision);
    }
}
