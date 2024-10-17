using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;
using System.Linq;
using System;
using CartoonFX;

public class Player : Previewable
{
    [SerializedDictionary]
    public SerializedDictionary<ButtonValue, InputRenderer> buttonValueDisplays;

    [Header("SFX")]
    public AudioClip moveSFX;
    public AudioClip unableToMoveSFX;
    public AudioClip fireSFX;
    public AudioClip scrambleSFX;
    public AudioClip exitSFX;

    public delegate void PossibleInputs(List<PlayerAction> possibleActions);
    public PossibleInputs OnPossibleInputs;

    [SerializedDictionary]
    protected SerializedDictionary<ButtonValue, PlayerAction> scrambledActions = new SerializedDictionary<ButtonValue, PlayerAction>();
    
    private PlayerShipInfo _shipInfo;
    private ParticleSystem _deathVFX;

    Sprite _shipSprite;
    SpriteRenderer _shipRenderer;
    Collider2D _shipCollider;
    AudioSource _shipAudio;
    TickDurationUI _tickDurationUI;

    public int PlayerId { get; private set; }
    private bool _allowingInput;
    private bool _isInactive = false;
    private bool _betweenTicks = false;

    ButtonValue? _lastInput;
    GameInputProgression? _lastGameProgression;
    InputMoveStyle _moveStyle;

    List<PlayerAction> _possibleActions = new List<PlayerAction>();
    List<Condition> _playerConditions = new List<Condition>();    

    private void Awake()
    {
        _shipCollider = GetComponent<Collider2D>();
    }

    private void OnDisable()
    {
        if (_manager != null)
        {
            _manager.EffectsSystem.OnInputMoveStyleChanged -= OnInputMoveStyleChanged;
            _manager.EffectsSystem.OnGameInputProgressionChanged -= OnGameInputProgressionChanged;
        }        
    }

    public virtual void InitPlayer(GameManager manager, PlayerShipInfo shipInfo, int id, InputMoveStyle style)
    {      
        _manager = manager;
        _manager.OnTickStart += OnTickStart;
        _manager.OnTickEnd += OnTickEnd;
        _manager.EffectsSystem.OnInputMoveStyleChanged += OnInputMoveStyleChanged;
        _manager.EffectsSystem.OnGameInputProgressionChanged += OnGameInputProgressionChanged;

        _shipInfo = shipInfo;
        PlayerId = id;
        _allowingInput = false;
        _moveStyle = style;

        _deathVFX = Instantiate(_shipInfo.deathVFX, transform);
        _deathVFX.gameObject.SetActive(false);
        _shipSprite = _shipInfo.shipSprite;
        _shipRenderer = GetComponentInChildren<SpriteRenderer>();
        _shipRenderer.sprite = _shipSprite;
        _shipAudio = GetComponentInChildren<AudioSource>();
        _tickDurationUI = GetComponentInChildren<TickDurationUI>();
        _tickDurationUI.SetupTickListening(manager);
    }

    private void OnInputMoveStyleChanged(InputMoveStyle style)
    {
        _moveStyle = style;
    }

    private void OnGameInputProgressionChanged(GameInputProgression scrambleType)
    {
        if (_lastGameProgression != null && _lastGameProgression.Value == scrambleType)
        {
            return;
        }
        
        _lastGameProgression = scrambleType;

        _possibleActions.Clear();
        AddPossibleInput(InputValue.Forward);
        AddPossibleInput(InputValue.Backward);
        AddPossibleInput(InputValue.Port);
        AddPossibleInput(InputValue.Starboard);

        switch (scrambleType)
        {            
            case GameInputProgression.Rotation:
                RemovePossibleInput(InputValue.Starboard);
                RemovePossibleInput(InputValue.Port);
                AddPossibleInput(InputValue.Clockwise);
                AddPossibleInput(InputValue.Counterclockwise);
                AddPossibleInput(InputValue.Fire);
                break;
            case GameInputProgression.MoveAndShooting:
            case GameInputProgression.ScrambledShooting:
                AddPossibleInput(InputValue.Fire);
                break;
            default:
                break;
        }

        OnPossibleInputs?.Invoke(_possibleActions);
    }

    protected virtual void OnTickStart(float _)
    {        
        if (_isInactive)
        {            
            return;
        }

        _allowingInput = true;
        _lastInput = null;
        _betweenTicks = false;

        foreach (var inputValue in buttonValueDisplays)
        {
            inputValue.Value.OnTickStart();
        }
    }

    protected virtual void OnTickEnd(float tickEndDuration, int nextTickNumber)
    {
        if (_isInactive) 
        {
            return;
        }

        _allowingInput = false;
        _betweenTicks = true;

        var hasActiveInput = HasActiveInput();
        foreach (var inputValue in buttonValueDisplays)
        {
            if (!hasActiveInput)
            {
                inputValue.Value.OnNoInputSelected();
            }
            else
            {
                if (_lastInput.Value == inputValue.Key)
                {
                    StartCoroutine(inputValue.Value.OnTickEnd(tickEndDuration));
                }
            }
        }

        //looping backwards like this allows us to safely remove items from the list
        for (int i = _playerConditions.Count - 1; i >= 0; i--) 
        {
            var condition = _playerConditions[i];
            condition.OnTickEnd();
        }
    }
    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        if (_isInactive)
        {
            return;
        }

        if (!_allowingInput)
        {
            if (_betweenTicks && context.performed)
            {
                PlayShipSFX(unableToMoveSFX);
            }
            return;
        }

        Vector2 playerMovement = context.ReadValue<Vector2>();

        if (_moveStyle == InputMoveStyle.OnInputEnd && context.canceled && _lastInput != ButtonValue.Action)
        {
            _manager.EndCurrentTick(this);
            return;
        }

        if (playerMovement == Vector2.zero || !context.performed)
        {
            return;
        }

        var playerMovementInput = SimplifyDirection(playerMovement);

        if(scrambledActions.TryGetValue(playerMovementInput, out var playerAction))
        {
            SendInput(playerMovementInput, playerAction);
        }
    }

    ButtonValue SimplifyDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                return ButtonValue.Right;              
            }
            else
            {

                return ButtonValue.Left;
            }
        }
        else
        {
            if (direction.y > 0)
            {
                return ButtonValue.Up;
            }
            else
            {
                return ButtonValue.Down;
            }
        }
    }

    public void OnPlayerFire(InputAction.CallbackContext context)
    {
        if (_isInactive)
        {
            return;
        }

        if (!_allowingInput)
        {
            if (_betweenTicks && context.performed)
            {
                PlayShipSFX(unableToMoveSFX);
            }
            return;
        }

        if (_moveStyle == InputMoveStyle.OnInputEnd && context.canceled && _lastInput == ButtonValue.Action)
        {
            _manager.EndCurrentTick(this);
            return;
        }

        if (!context.performed)
        {
            return;
        }

        if (scrambledActions.TryGetValue(ButtonValue.Action, out var playerAction))
        {
            SendInput(ButtonValue.Action, playerAction);
        }
    }

    //Takes the input pressed and the action that press triggered
    protected void SendInput(ButtonValue pressedValue, PlayerAction playerAction)
    {
        if (_lastInput == pressedValue)
        {
            return;
        }

        UpdateLastInput(pressedValue);
        _manager.ClearPreviousPlayerAction(this);
        var targetTile = _manager.GetTileForPlayerAction(playerAction);

        if (targetTile != null && targetTile.IsVisible)
        {
            PreviewAction newPreview;
            Player playerActedUpon = playerAction.playerActionPerformedOn;

            if (playerAction.inputValue == InputValue.Fire)
            {
                if (playerActedUpon._shipInfo.fireable.TryGetComponent<Bullet>(out var bullet))
                {
                    var bulletGridMoveable = CreateBullet(playerActedUpon, targetTile);
                    newPreview = _manager.CreatePreviewOfPreviewableAtTile(bulletGridMoveable, targetTile);
                    newPreview.creatorOfPreview = this;
                }
                else
                {
                    newPreview = new PreviewAction();
                }
            }
            else
            {
                var inputValue = playerAction.inputValue;
                var rotation = ConvertInputValueToRotation(inputValue);
                newPreview = _manager.CreatePreviewOfPreviewableAtTile(playerActedUpon, targetTile, rotation);
            }

            _manager.AddPlayerPreviewAction(this, newPreview);

            if (_moveStyle == InputMoveStyle.OnInputStart)
            {
                _manager.EndCurrentTick(this);
            }
        }
    }

    void UpdateLastInput(ButtonValue pressedValue)
    {
        if (_lastInput != null)
        {
            buttonValueDisplays[_lastInput.Value].DeselectInput();
        }

        _lastInput = pressedValue;
        buttonValueDisplays[_lastInput.Value].SelectInput();
    }

    GridMovable CreateBullet(Player firingPlayer, Tile spawnTile)
    {
        var bulletGridMoveable = _manager.CreateMovableAtTile(firingPlayer._shipInfo.fireable, firingPlayer, spawnTile);
        bulletGridMoveable.gameObject.transform.localRotation = GetTransfromAsReference().localRotation;
        var bullet = bulletGridMoveable.GetComponent<Bullet>();
        bullet.spawnSound = fireSFX;
        bullet.PreviewColor = _shipInfo.baseColor;
        bullet.owner = this;
        bulletGridMoveable.GetComponentInChildren<SpriteRenderer>().sprite = _shipInfo.bulletSprite;

        return bulletGridMoveable;
    }

    public bool IsPlayerDeadOnHit()
    {
        if (_isInactive)
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
        _deathVFX.gameObject.SetActive(true);
        _deathVFX.Play();
        var cfxrEffects = _deathVFX.GetComponentsInChildren<CFXR_Effect>();
        foreach (var cfxr in cfxrEffects)
        {
            cfxr.ResetState();
        }
        _isInactive = true;
        SetShipVisiblity(false);
        SetInputVisibility(false);
    }

    public void OnSpawn()
    {
        _isInactive = false;
        _manager.OnTickStart += ShowVisiblity;
        SetInputStatus(false);
        SetShipVisiblity(true);

        //use a local function to queue up a function for the next call of a event
        void ShowVisiblity(float _)
        {
            SetInputStatus(true);
            _manager.OnTickStart -= ShowVisiblity;
        }
    }

    public void OnGameOver()
    {
        _isInactive = true;
    }

    public List<PlayerAction> GetPossibleActions()
    {
        return _possibleActions;
    }

    public void AddPossibleInput(InputValue inputToAdd)
    {
        if (_possibleActions.Count(x => x.inputValue == inputToAdd) > 0)
        {
            return;
        }

        _possibleActions.Add(new PlayerAction()
        {
            playerActionPerformedOn = this,
            inputValue = inputToAdd,
        });
    }

    public void RemovePossibleInput(InputValue inputToRemove)
    {
        _possibleActions.RemoveAll(x => x.inputValue == inputToRemove);
    }

    public void SetScrambledActions(SerializedDictionary<ButtonValue, PlayerAction> playerActions)
    {
        if (playerActions.Keys.Count == 0)
        {
            return;
        }

        scrambledActions = playerActions;
        var playerActionKeys = playerActions.Keys;
        var hasAnyRenderersChanged = false;

        foreach (var item in playerActionKeys)
        {
            buttonValueDisplays.TryGetValue(item, out var renderer);

            if (renderer != null)
            {
                var playerAction = playerActions[item];
                var input = playerAction.inputValue;
                var newSprite = playerAction.playerActionPerformedOn.GetSpriteForInput(input);

                if (!hasAnyRenderersChanged)
                {
                    hasAnyRenderersChanged = renderer.WillSpriteChangeVisibly(newSprite);
                }

                renderer.SetSprite(newSprite);
            }
        }

        if (hasAnyRenderersChanged)
        {
            PlayShipSFX(scrambleSFX);
        }
    }

    public List<PlayerAction> GetScrambledActions()
    {
        if (scrambledActions == null)
        {
            return _possibleActions;
        }

        return scrambledActions.Values.ToList();
    }

    public void SetInputStatus(bool isActive)
    {
        _allowingInput = isActive;
        SetInputVisibility(isActive);

        if (_shipCollider == null)
        {
            _shipCollider = GetComponent<Collider2D>();
        }

        _shipCollider.enabled = isActive;
    }

    public void SetActiveStatus(bool isActive)
    {
        _isInactive = !isActive;
    }

    public void OnMoveOnScreen()
    {
        _betweenTicks = false;
    }

    public void OnMoveOffScreen()
    {
        PlayShipSFX(exitSFX);
        _betweenTicks = false;
    }

    void SetShipVisiblity(bool isVisible)
    {
        _shipRenderer.enabled = isVisible;

        if (_shipCollider == null)
        {
            _shipCollider = GetComponent<Collider2D>();
        }
        _shipCollider.enabled = isVisible;
    }

    void SetInputVisibility(bool isVisible)
    {
        foreach (var item in buttonValueDisplays)
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

    public bool FindCondition<T>(out Condition condition) where T : Condition
    {
        bool hasCondition = _playerConditions.Any(x => x.GetType() == typeof(T));
        if (hasCondition)
        {
            condition = _playerConditions.First(x => x.GetType() == typeof(T));
        }
        else
        {
            condition = null;
        }

        return hasCondition;
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

    public override Color GetPreviewOutline()
    {
        return _shipInfo.baseColor;
    }

    public Sprite GetBulletSprite()
    {
        return _shipInfo.bulletSprite;
    }

    public Sprite GetActionButtonSprite()
    { 
        return _shipInfo.actionButtonSprite;
    }

    public Sprite GetSpriteForInput(InputValue input)
    {
        if (_shipInfo.inputsForSprites.TryGetValue(input, out var sprite))
        {
            return sprite;
        }

        return null;
    }

    public bool HasActiveInput()
    { 
        return _lastInput != null;
    }

    public bool IsActive()
    {
        return !_isInactive;
    }

    public void AddButtonRenderer(ButtonValue value, InputRenderer renderer)
    {
        buttonValueDisplays.Add(value, renderer);
    }

    protected override void PerformInteraction(GridObject collidedGridObject)
    {
        if (collidedGridObject.TryGetComponent<Player>(out var player))
        {
            _manager.HandlePlayerCollision(this, player);
        }

        base.PerformInteraction(collidedGridObject);
    }

    public override Transform GetTransfromAsReference()
    {
       return _shipRenderer.transform;
    }

    //rotate the ship image itself, rather than the whole ship. Rotating the whole ship messes with the UI
    public override void TransitionToRotation(Quaternion newRotation, float duration)
    {
        if (_transitioner == null)
        {
            _transitioner = GetComponent<TransformTransition>();
        }

        _transitioner.RotateTo(newRotation, duration, GetTransfromAsReference());
    }

    public override void ResolvePreviewable()
    {
        PlayShipSFX(moveSFX);
    }

    private void PlayShipSFX(AudioClip clipToPlay)
    {
        _shipAudio.Stop();
        _shipAudio.clip = clipToPlay;
        _shipAudio.Play();
    }
}
