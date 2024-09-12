using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public Player playerShip;
    public List<PlayerShipInfo> shipInfos = new List<PlayerShipInfo>();
    public PreviewableBase previewableBase;

    //GameManager Events
    public delegate void LevelStart(int levelId);
    public LevelStart OnLevelStart;

    public delegate void LevelEnd(int energyLeft, int continuesUsed);
    public LevelEnd OnLevelEnd;

    public delegate void TickStart(float timeToTickEnd);
    public TickStart OnTickStart;

    public delegate void TickEnd(int nextTickNumber);
    public TickEnd OnTickEnd;

    public delegate void ScreenChange(int nextScreenIndex, int maxScreens);
    public ScreenChange OnScreenChange;

    public delegate void ScreenResetStart();
    public ScreenResetStart OnScreenResetStart;

    public delegate void ScreenResetEnd();
    public ScreenResetEnd OnScreenResetEnd;

    public delegate void GameStateChanged(GameState newState);
    public GameStateChanged OnGameStateChanged;

    public delegate void PlayerJoined(Player player);
    public PlayerJoined OnPlayerJoinedGame;

    public delegate void PlayerDeath(Player player);
    public PlayerDeath OnPlayerDeath;

    public delegate void PlayerConditionStart(Player player, Condition condition);
    public PlayerConditionStart OnPlayerConditionStart;

    public delegate void PlayerConditionEnd(Player player, Condition condition);
    public PlayerConditionEnd OnPlayerConditionEnd;

    public delegate void PlayerPickupStart(Player player, PickupType pickupType);
    public PlayerPickupStart OnPlayerPickup;

    //Private Variables
    GameState _currentGameState = GameState.Waiting;
    GameState _previousGameState = GameState.Waiting;
    Dictionary<Player, PreviewAction> _attemptedPlayerActions = new Dictionary<Player, PreviewAction>();
    List<PreviewAction> _previewActions = new List<PreviewAction>();
    private List<Player> _players = new List<Player>();
    private int _playerCount = 0;
    private Level _currentLevel;
    List<GridCoordinate> _startingPlayerPositions;

    GridSystem _gridSystem;
    SpawnSystem _spawnSystem;
    DialogueSystem _dialogueSystem;
    CutsceneSystem _cutsceneSystem;
    ScreenSystem _screenSystem;
    EnergySystem _energySystem;
    EffectsSystem _effectsSystem;
    public EffectsSystem EffectsSystem { get => _effectsSystem; }
    public EnergySystem EnergySystem { get => _energySystem; }

    float _tickDuration = .5f;
    float _tickEndDuration = .5f / 5;
    float TickDuration
    {
        get => _tickDuration;
        set
        {
            _tickDuration = value;
        }
    }
    bool _isMovementAtInput = false;

    float _tickElapsed = 0f;
    int _playerFinishedWithScreen = 0;
    bool _tickIsOccuring = false;
    int _ticksSinceScreenStart = 0;
    int _ticksSinceLevelStart = 0;
    int _continuesUsed = 0;

    ScrambleType _currentScrambleType;

    void Awake()
    {
        _gridSystem = GetComponent<GridSystem>();
        _spawnSystem = GetComponent<SpawnSystem>();
        _dialogueSystem = GetComponent<DialogueSystem>();
        _cutsceneSystem = GetComponent<CutsceneSystem>();
        _effectsSystem = GetComponent<EffectsSystem>();
        _screenSystem = GetComponent<ScreenSystem>();
        _energySystem = GetComponent<EnergySystem>();

        _effectsSystem.OnTickDurationChanged += (float newDuration) => TickDuration = newDuration;
        _effectsSystem.OnTickEndDurationChanged += (float newEndDuration) => _tickEndDuration = newEndDuration;
        _effectsSystem.OnMoveOnInputChanged += (bool isMoveOnInput) => _isMovementAtInput = isMoveOnInput;
        _effectsSystem.OnScrambleTypeChanged += (ScrambleType scrambleType) => _currentScrambleType = scrambleType;
    }

    IEnumerator Start()
    {       
        foreach (IManager managerObjects in FindAllManagers())
        {
            managerObjects.InitManager(this);
        }

        UpdateGameState(GameState.Waiting);
        yield return new WaitForSeconds(.125f); //TODO: Fix race condition
        OptionsManager.Instance.AfterInitManager();

        _currentLevel = GlobalGameStateManager.Instance.GetLevelInfo();
        _playerCount = GlobalGameStateManager.Instance.PlayerCount;

        if (_playerCount == 0)
        {
            _playerCount = 1;
        }

        _screenSystem.SetScreens(_currentLevel, _playerCount);
        _startingPlayerPositions = _screenSystem.GetStartingPlayerPositions(_playerCount);

        CreatePlayerShip();

        if (_playerCount == 2)
        {
            CreatePlayerShip();
        }

        _energySystem.SetEnergy(_playerCount);
        _screenSystem.TriggerStartingEffects(_effectsSystem);
        StartCoroutine(SetupNextScreen(TickDuration, false));
        UpdateGameState(GameState.Transition);
    }

    private List<IManager> FindAllManagers()
    {
        IEnumerable<IManager> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IManager>();

        return new List<IManager>(dataPersistenceObjects);
    }

    private void CreatePlayerShip()
    {
        var playerObject = Instantiate(playerShip);
        var newPlayer = playerObject.GetComponent<Player>();

        int playerId = _players.Count;
        bool isShootingEnabled = OptionsManager.Instance.gameSettingParameters.isShootingEnabled;
        newPlayer.InitPlayer(this, shipInfos[playerId], playerId, isShootingEnabled);
        newPlayer.transform.SetParent(transform);

        _players.Add(newPlayer);

        var startingTile = GetStartingTileForPlayer(playerId);
        MovePreviewableOffScreenToTile(playerObject, startingTile, 0);
        newPlayer.OnSpawn();
        OnPlayerJoinedGame?.Invoke(newPlayer);
    }

    Tile GetStartingTileForPlayer(int playerId)
    {
        var startingPosition = _startingPlayerPositions[playerId];

        _gridSystem.TryGetTileByCoordinates(startingPosition, out var startingTile);
        return startingTile;
    }

    public void MovePreviewableOffScreenToTile(Previewable preview, Tile tile, float duration)
    {
        _spawnSystem.MovePreviewableOffScreenToPosition(preview, preview.transform.up, tile.GetTilePosition(), duration);
    }

    void MovePlayerOnScreenToTile(Player player, Tile tile, float duration)
    {
        _spawnSystem.MovePreviewableOffScreenToPosition(player, player.transform.up, tile.GetTilePosition(), 0, true);
        player.TransitionToTile(tile, duration);
        player.OnMoveOnScreen();
    }

    void MovePlayerOntoStartingTitle(Player player, float duration)
    {
        var startingTile = GetStartingTileForPlayer(player.PlayerId);
        MovePlayerOnScreenToTile(player, startingTile, duration);
    }

    public void PlayerGainedCondition(Player player, Condition condition)
    { 
        OnPlayerConditionStart?.Invoke(player, condition);
    }

    public void PlayerLostCondition(Player player, Condition condition)
    {
        OnPlayerConditionEnd?.Invoke(player, condition);
    }

    public void PlayerPickup(Player player, PickupType pickupType)
    { 
        OnPlayerPickup?.Invoke(player, pickupType);
    }

    public void ActivateCutscene(CutsceneType type, float cutsceneDuration)
    {
        if (_currentGameState == GameState.Cutscene)
        {
            return;
        }

        ToggleIsPlaying(false, GameState.Cutscene);
        _cutsceneSystem.ActivateCutscene(type, cutsceneDuration);
    }

    /// <summary>
    /// Screen Change Trigger occurs when a player hits the screen change trigger
    /// </summary>
    /// <param name="player"></param>
    public void ScreenChangeTriggered(Player player)
    {
        //disable the player who touched it and move them offscreen
        //move player off screen
        var currentPos = player.CurrentTile.GetTilePosition();
        _spawnSystem.MovePreviewableOffScreenToPosition(player, player.transform.up, currentPos, TickDuration);
        player.OnMoveOffScreen();

        _playerFinishedWithScreen++;

        if (_playerFinishedWithScreen >= _players.Count)
        {
            _playerFinishedWithScreen = 0;
            ClearAllPreviews();
            ToggleIsPlaying(false, GameState.Transition);
            EndScreen(TickDuration);
        }
        else
        {
            player.SetInputStatus(false);
            player.SetActiveStatus(false);
        }        
    }

    void EndScreen(float endingDuration)
    {
        int screensRemainingInLevel = _screenSystem.ScreenAmount - _screenSystem.ScreensLoaded;
        if (screensRemainingInLevel <= 0)
        {
            UpdateGameState(GameState.Win);
            OnLevelEnd?.Invoke(_energySystem.CurrentEnergy, _continuesUsed);                                
        }
        else
        {
            StartCoroutine(SetupNextScreen(endingDuration));
        }
    }

    IEnumerator SetupNextScreen(float tickDuration, bool playTransitionCutscene = true)
    {
        OnScreenChange?.Invoke(_screenSystem.ScreensLoaded, _screenSystem.ScreenAmount);       

        if (playTransitionCutscene)
        {
            ActivateCutscene(CutsceneType.ScreenTransition, tickDuration);
            yield return new WaitForSeconds(tickDuration);
        }

        _screenSystem.SetupNewScreen(_spawnSystem, _gridSystem, _effectsSystem, _dialogueSystem);
        _startingPlayerPositions = _screenSystem.GetStartingPlayerPositions(_playerCount);
        _ticksSinceScreenStart = 0;
        yield return new WaitForSeconds(tickDuration);

        //move ships on screen
        foreach (Player player in _players)
        {
            MovePlayerOntoStartingTitle(player, tickDuration);
        }

        yield return new WaitForSeconds(tickDuration);

        UpdateGameState(GameState.Dialogue);
        if (_dialogueSystem.HasDialogue())
        {
            _dialogueSystem.StartDialogue();
            _dialogueSystem.OnDialogueEnd += WaitUntilDialogueEnds;
            void WaitUntilDialogueEnds()
            {
                _dialogueSystem.OnDialogueEnd -= WaitUntilDialogueEnds;
                StartCoroutine(DelayUnpausing(_tickEndDuration * 2));
            }
        }
        else
        {
            ToggleIsPlaying(true);
        }
    }

    IEnumerator DelayUnpausing(float delayAmount)
    { 
        yield return new WaitForSeconds(delayAmount);
        ToggleIsPlaying(true);
    }

    public void ToggleIsPlaying(bool isPlaying, GameState disabledState = GameState.Paused)
    {
        foreach (Player player in _players)
        {
            player.SetInputStatus(isPlaying);
        }

        var newGameState = (isPlaying) ? GameState.Playing : disabledState;
        UpdateGameState(newGameState);
    }

    public void PauseGame()
    {
        if (_currentGameState == GameState.Paused)
        {
            if (_previousGameState == GameState.Playing)
            {
                ToggleIsPlaying(true);
            }
            else
            {
                UpdateGameState(_previousGameState);
            }
        }
        else if (_currentGameState == GameState.Playing)
        {
            ToggleIsPlaying(false);
        }
        else
        {
            UpdateGameState(GameState.Paused);
        }
    }

    public void ContinuePerformed()
    {
        if (_currentGameState == GameState.Win || _currentGameState == GameState.GameOver)
        {
            StopAllCoroutines();
            GlobalGameStateManager.Instance.ConsumeCredits(_playerCount);
            GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.Game;
            _energySystem.RefillEnergy();
            _continuesUsed++;
            StartCoroutine(ResetScreen(true));
        }
    }

    void UpdateGameState(GameState gameState) 
    {
        _previousGameState = _currentGameState;
        _currentGameState = gameState;

        switch (gameState) 
        {
            case GameState.Playing:
                StartNextTick();
                break;
            case GameState.Dialogue:
                _dialogueSystem.SetDialogueIsEnable(true);
                break;
            case GameState.Paused:
                _dialogueSystem.SetDialogueIsEnable(false);
                break;
            case GameState.Transition:
            case GameState.GameOver: //this might run into a race condition with on tick end
                _tickIsOccuring = false;
                break;
        }

        if (_currentGameState == GameState.Paused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        OnGameStateChanged?.Invoke(_currentGameState);
    }

    public void HandlePlayerCollision(Player playerAttack, Player playerHit)
    {
        PlayerCollision(playerAttack);
        PlayerCollision(playerHit);
    }

    public void HandleGridObjectCollision(GridObject attacking, GridObject hit) //Should this be here in this state? Feels like something the grid object itself should be in charge of
    {
        attacking.DestroyObject();

        if (hit.TryGetComponent<Player>(out var player))
        {
            PlayerCollision(player);
        }
        else
        {
            if (hit.CanBeShot())
            {
                hit.DestroyObject();
            }
        }
    }

    void PlayerCollision(Player player)
    {
        bool canPlayerDie = player.OnHit();
        if (canPlayerDie)
        {
            player.OnDeath();            
            OnPlayerDeath?.Invoke(player);

            if (_energySystem.CanPlayerDieAndGameContinue())
            {
                StartCoroutine(ResetScreen(false));
            }
            else
            {
                OnGameOver();
            }
        }
    }

    void OnGameOver()
    {
        if (_currentGameState != GameState.Playing)
        {
            return;
        }

        ClearAllPreviews();
        _energySystem.CurrentEnergy = 0;
        UpdateGameState(GameState.GameOver);
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.GameOver;
    }

    IEnumerator ResetScreen(bool isOnContinue)
    {
        ToggleIsPlaying(false, GameState.Transition);
        ClearAllPreviews();

        if (!isOnContinue)
        {
            yield return new WaitForSeconds(_tickDuration * 2);
        }

        OnScreenResetStart?.Invoke();
        _cutsceneSystem.PerformRewindEffect();

        yield return new WaitForSeconds(_tickDuration);
        _effectsSystem.PerformEffect(EffectType.DigitalGlitchIntensity, 0);
        _effectsSystem.PerformEffect(EffectType.ScanLineJitter, 0);
        _effectsSystem.PerformEffect(EffectType.HorizontalShake, 0);

        _playerFinishedWithScreen = 0;
        _ticksSinceScreenStart = 0;
        _screenSystem.ConfigureCurrentScreen(_spawnSystem, _gridSystem, _effectsSystem);

        foreach (Player player in _players)
        {
            player.OnSpawn();
            MovePlayerOntoStartingTitle(player, _tickDuration);
        }

        if (!isOnContinue)
        {
            OnScreenResetEnd?.Invoke();
        }

        yield return new WaitForSeconds(_tickDuration);
        ToggleIsPlaying(true);
    }

    public List<Player> GetAllPlayers()
    {
        return _players;
    }

    public int GetPlayersRemaining()
    {
        return _playerCount - _playerFinishedWithScreen;
    }

    public void ClearPreviousPlayerAction(Player playerSent)
    {
        //change this so that we look at what the player sent, then delete said preview
        if (_attemptedPlayerActions.TryGetValue(playerSent, out var previousPreview))
        {
            //need to delete the item they created if it isn't made
            _previewActions.Remove(previousPreview);
            _attemptedPlayerActions.Remove(playerSent);

            Destroy(previousPreview.sourcePreviewable.previewObject); //look into using pooling instead

            if (previousPreview.creatorOfPreview)
            {
                Destroy(previousPreview.sourcePreviewable.gameObject);
            }
        }
    }

    public Tile GetTileForPlayerAction(PlayerAction playerInputValue)
    {
        var targetPlayer = playerInputValue.playerActionPerformedOn;
        //get the player from the action, since that's who is performing the action. A player might be performing an action on someone else
        return GetTileFromInput(targetPlayer, playerInputValue.inputValue);
    }

    public PreviewAction CreatePreviewOfPreviewableAtTile(Previewable previewableObject, Tile previewTile, int duration = 0, bool isMoving = true)
    {
        var preview = _spawnSystem.CreateSpawnObject(previewableBase.gameObject, previewTile, previewableObject.transform.rotation);
        preview.name = $"Preview of {previewableObject}";
        preview.transform.localScale = previewableObject.GetPreviewScale();
        var previewImage = previewableObject.GetPreviewSprite();

        var renderer = preview.GetComponent<SpriteRenderer>();
        renderer.sprite = previewImage;
        renderer.color = previewableObject.GetPreviewColor();
        preview.GetComponent<PreviewableBase>().SetPreviewOutlineColor(previewableObject.GetPreviewOutline(), previewImage);
        previewableObject.previewObject = preview;

        return new PreviewAction()
        {
            sourcePreviewable = previewableObject,
            isNotMoving = !isMoving,
            previewTile = previewTile,
            previewFinishedTick = _ticksSinceScreenStart + duration,
        };    
    }

    public GridMovable CreateMovableAtTile(GridMovable movableToBeCreated, Previewable previewableCreatingMovable, Tile previewTile, Vector2 movingDirection)
    {
        var spawnedMovable = _spawnSystem.CreateSpawnObject(movableToBeCreated.gameObject, previewableCreatingMovable.CurrentTile, previewableCreatingMovable.transform.rotation);
        var moveable = spawnedMovable.GetComponent<GridMovable>();
        moveable.SetupMoveable(this, _spawnSystem, previewTile);
        moveable.travelDirection = movingDirection;
        moveable.gameObject.SetActive(false);

        return moveable;
    }

    public Tile AddPreviewAtPosition(Previewable previewObject, Tile currentTile, Vector2 previewDirection)
    {
        var possibleGridCoordinates = previewDirection + currentTile.GetTileCoordinates();
        var isPossibleTileSpace = _gridSystem.TryGetTileByCoordinates(possibleGridCoordinates.x, possibleGridCoordinates.y, out Tile tile);

        PreviewAction newPreview;
        if (!isPossibleTileSpace)
        {
            return null;           
        }

        newPreview = CreatePreviewOfPreviewableAtTile(previewObject, tile);

        _previewActions.Add(newPreview);
        return tile;
    }

    public void AddPlayerPreviewAction(Player playerPerformingAction, PreviewAction newPreview)
    {
        _attemptedPlayerActions.Add(playerPerformingAction, newPreview);
        AddPreviewAction(newPreview);

        if (_isMovementAtInput && (_attemptedPlayerActions.Count + _playerFinishedWithScreen) >= _players.Count) //wait until all the players have inputted before advancing
        {
            _tickElapsed = TickDuration;
        }
    }

    public void AddPreviewAction(PreviewAction preview)
    { 
        _previewActions.Add(preview);
    }

    void Update()
    {
        if (!_tickIsOccuring || _currentGameState != GameState.Playing)
        {
            return;
        }

        _tickElapsed += Time.deltaTime;
        if (_tickElapsed >= TickDuration) 
        {
            StartCoroutine(SetupNewTick());
        }
    }

    IEnumerator SetupNewTick()
    {
        _tickIsOccuring = false;
        var tickEndDuration = _tickEndDuration;
        if (_currentScrambleType == ScrambleType.None) //if we're not scrambled, speed up the animation between ticks because people know their next input will be
        {
            tickEndDuration /= 1.5f;
        }

        EndCurrentTick(tickEndDuration);
        yield return new WaitForSeconds(tickEndDuration);

        if (_energySystem.CurrentEnergy <= 0)
        {
            OnGameOver();
        }

        //something might have occured during the tick end where the game state changed from playing, so only start another tick if we're still playing
        if (_currentGameState == GameState.Playing)
        {
            StartNextTick();
        }
    }

    void EndCurrentTick(float tickEndDuration)
    {
        foreach (var preview in _previewActions)
        {
            if (preview.isNotMoving)
            {
                continue;
            }

            var movingObject = preview.sourcePreviewable;

            if (movingObject == null)
            {
                continue;
            }

            if (preview.creatorOfPreview)
            {
                movingObject.OnPreviewableCreation();
                preview.creatorOfPreview.CreatedNewPreviewable(movingObject);

                if (preview.creatorOfPreview.TryGetComponent<Player>(out var player))
                {
                    _energySystem.OnPlayerFired();
                }
            }

            movingObject.TransitionToTile(preview.previewTile, tickEndDuration);
            movingObject.ResolvePreviewable();
        }
        
        OnTickEnd?.Invoke(_ticksSinceScreenStart);
    }

    void StartNextTick()
    {
        ClearAllPreviews();
        OnTickStart?.Invoke(TickDuration);
        _tickIsOccuring = true;
        _ticksSinceScreenStart++;
        _ticksSinceLevelStart++;
        _tickElapsed = 0;
    }

    void ClearAllPreviews()
    {
        //check if we should remove the preview, rather than always removing it
        for (int index = _previewActions.Count - 1; index >= 0; index--)
        {
            var preview = _previewActions[index];
            if (preview.previewFinishedTick - _ticksSinceScreenStart <= 0)
            {
                Destroy(preview.sourcePreviewable.previewObject);
                _previewActions.Remove(preview);
            }
        }

        _attemptedPlayerActions.Clear();
    }

    public Tile GetTileFromInput(Previewable inputSource, InputValue input)
    {
        var targetCoordinates = inputSource.GetGridCoordinates();
        switch (input)
        {
            case InputValue.Forward:
            case InputValue.Fire:
                targetCoordinates += (Vector2)inputSource.transform.up;
                break;
            case InputValue.Backward:
                targetCoordinates += (Vector2)inputSource.transform.up * -1;
                break;
            case InputValue.Port:
                targetCoordinates += (Vector2)inputSource.transform.right * -1;
                break;
            case InputValue.Starboard:
                targetCoordinates += (Vector2)inputSource.transform.right;
                break;
            default:
                break;
        }

        _gridSystem.TryGetTileByCoordinates(targetCoordinates, out var tile);

        return tile;
    }

    public float GetTimeRemainingInTick()
    {
        if (_currentGameState != GameState.Playing)
        {
            return 0f;
        }

        float timeRemaining = TickDuration - _tickElapsed;
        if (timeRemaining < 0)
        { 
            timeRemaining = 0;
        }
        return timeRemaining;
    }
}

public struct PreviewAction
{ 
    public Previewable sourcePreviewable;
    #nullable enable
    public Previewable? creatorOfPreview;
    #nullable disable
    public Tile previewTile;
    public bool isNotMoving;
    public int previewFinishedTick;
}

public enum GameState
{ 
    Waiting,
    Playing,
    Paused,
    Transition,
    Cutscene,
    Dialogue,
    GameOver,
    Win
}
