using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Player playerShip;
    public List<ShipInfo> shipInfos = new List<ShipInfo>();
    public SerializedDictionary<int, List<GridCoordinate>> _startingPlayerPositions;
    public PreviewableBase previewableBase;

    //GameManager Events
    public delegate void LevelStart(int levelId);
    public LevelStart OnLevelStart;

    public delegate void LevelEnd(int ticksPassed, float tickDuration);
    public LevelEnd OnLevelEnd;

    public delegate void TickStart(float timeToTickEnd);
    public TickStart OnTickStart;

    public delegate void TickEnd(int nextTickNumber);
    public TickEnd OnTickEnd;

    public delegate void ScreenChange(int screensRemaining);
    public ScreenChange OnScreenChange;

    public delegate void GameStateChanged(GameState newState);
    public GameStateChanged OnGameStateChanged;

    public delegate void PlayerJoined(Player player, int numberOfLives);
    public PlayerJoined OnPlayerJoinedGame;

    public delegate void PlayerDeath(Player player, int livesLeft);
    public PlayerDeath OnPlayerDeath;

    public delegate void PlayerConditionStart(Player player, Condition condition);
    public PlayerConditionStart OnPlayerConditionStart;

    public delegate void PlayerConditionEnd(Player player, Condition condition);
    public PlayerConditionEnd OnPlayerConditionEnd;

    //Private Variables
    GameState _currentGameState = GameState.Waiting;
    Dictionary<Player, PreviewAction> _attemptedPlayerActions = new Dictionary<Player, PreviewAction>();
    List<PreviewAction> _previewActions = new List<PreviewAction>();
    private List<Player> _players = new List<Player>();
    private int _playerLives = 0;

    GridSystem _gridSystem;
    SpawnSystem _spawnSystem;
    DialogueSystem _dialogueSystem;
    CutsceneSystem _cutsceneSystem;
    ScreenSystem _screenSystem;
    EffectsSystem _effectsSystem;
    public EffectsSystem EffectsSystem { get => _effectsSystem; }

    float _tickDuration = .5f;
    float _tickEndDuration = .5f / 4;
    float TickDuration
    {
        get => _tickDuration;
        set
        {
            _tickDuration = value;
            _tickEndDuration = value / 4;
        }
    }
    bool _isMovementAtInput = false;

    float _tickElapsed = 0f;
    int _playerFinishedWithScreen;
    bool _tickIsOccuring = false;
    int _ticksSinceScreenStart = 0;
    int _ticksSinceLevelStart = 0;

    int _lastIndexForScrambling = 4;

    void Awake()
    {
        _gridSystem = GetComponent<GridSystem>();
        _spawnSystem = GetComponent<SpawnSystem>();
        _dialogueSystem = GetComponent<DialogueSystem>();
        _cutsceneSystem = GetComponent<CutsceneSystem>();
        _effectsSystem = GetComponent<EffectsSystem>();
        _screenSystem = GetComponent<ScreenSystem>();

        _effectsSystem.OnTickDurationChanged += (float newDuration) => TickDuration = newDuration;
        _effectsSystem.OnMoveOnInputChanged += (bool isMoveOnInput) => _isMovementAtInput = isMoveOnInput;
        _effectsSystem.OnScrambleAmountChanged += (int scrambleAmount) =>
        {
            if (scrambleAmount == 5)
            {
                _lastIndexForScrambling = 5;
            }
            else
            {
                _lastIndexForScrambling = 4;
            }
        };
    }

    IEnumerator Start()
    {       
        foreach (IManager managerObjects in FindAllManagers())
        {
            managerObjects.InitManager(this);
        }

        yield return new WaitForSeconds(.125f);
        var numberOfLives = OptionsManager.Instance.gameSettingParameters.amountLivesPerPlayer;
        OptionsManager.Instance.AfterInitManager();

        CreatePlayerShip(numberOfLives);

        if (GlobalGameStateManager.Instance.PlayerCount == 2)
        {
            CreatePlayerShip(numberOfLives);
        }

        var activeLevel = GlobalGameStateManager.Instance.GetLevelInfo();
        _screenSystem.SetScreens(activeLevel, _players.Count);

        StartCoroutine(SetupNextScreen(_screenSystem.GetScreensRemaining(), TickDuration, false));
        UpdateGameState(GameState.Transition);
    }

    private List<IManager> FindAllManagers()
    {
        IEnumerable<IManager> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IManager>();

        return new List<IManager>(dataPersistenceObjects);
    }

    private void CreatePlayerShip(int numberOfLives)
    {
        var playerObject = Instantiate(playerShip);
        var newPlayer = playerObject.GetComponent<Player>();

        int playerId = _players.Count;
        bool isShootingEnabled = OptionsManager.Instance.gameSettingParameters.isShootingEnabled;
        newPlayer.InitPlayer(this, shipInfos[playerId], playerId, isShootingEnabled);
        newPlayer.transform.SetParent(transform);

        _players.Add(newPlayer);
        _playerLives = _players.Count * numberOfLives;

        var startingTile = GetStartingTileForPlayer(_players.Count, playerId);
        MovePreviewableOffScreenToTile(playerObject, startingTile, 0);
        newPlayer.OnSpawn();
        OnPlayerJoinedGame?.Invoke(newPlayer, numberOfLives);
    }

    Tile GetStartingTileForPlayer(int playerAmount, int playerId)
    {
        var startingPosition = _startingPlayerPositions[playerAmount][playerId];

        _gridSystem.TryGetTileByCoordinates(startingPosition, out var startingTile);
        return startingTile;
    }

    public void MovePreviewableOffScreenToTile(Previewable preview, Tile tile, float duration)
    {
        _spawnSystem.MovePreviewableOffScreenToPosition(preview, preview.transform.up, tile.GetTilePosition(), duration);
    }

    public void MovePlayerOnScreenToTile(Player player, Tile tile, float duration)
    {
        _spawnSystem.MovePreviewableOffScreenToPosition(player, player.transform.up, tile.GetTilePosition(), 0, true);
        player.TransitionToTile(tile, duration);
    }

    void MovePlayerOntoStartingTitle(Player player, float duration)
    {
        var startingTile = GetStartingTileForPlayer(_players.Count, player.PlayerId);
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

    public int GetLivesRemaining()
    {
        return _playerLives;
    }

    public void ActivateCutscene(CutsceneType type, float cutsceneDuration)
    {
        if (_currentGameState == GameState.Cutscene)
        {
            return;
        }

        UpdateGameState(GameState.Cutscene);
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

    void EndScreen(float endingDuation)
    {
        int screensRemainingInLevel = _screenSystem.GetScreensRemaining();
        if (screensRemainingInLevel <= 0)
        {
            if (GlobalGameStateManager.Instance.IsActiveLevelTutorial())
            {
                StartCoroutine(PlayFirstCutscene(endingDuation));
            }
            else
            {
                OnLevelEnd?.Invoke(_ticksSinceLevelStart, TickDuration);
                UpdateGameState(GameState.Win);
            }            
        }
        else
        {
            StartCoroutine(SetupNextScreen(screensRemainingInLevel, TickDuration));
        }
    }

    IEnumerator PlayFirstCutscene(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        GlobalGameStateManager.Instance.PlayCutscene();
    }

    IEnumerator SetupNextScreen(int screensRemainingInLevel, float screenLoadDuration, bool playTransitionCutscene = true)
    {
        OnScreenChange?.Invoke(screensRemainingInLevel);       

        if (playTransitionCutscene)
        {
            ActivateCutscene(CutsceneType.ScreenTransition, screenLoadDuration);
            yield return new WaitForSeconds(screenLoadDuration);
        }

        _screenSystem.SetupNewScreen(_spawnSystem, _gridSystem, _effectsSystem, _dialogueSystem);
        _ticksSinceScreenStart = 0;
        yield return new WaitForSeconds(screenLoadDuration);

        //move ships on screen
        foreach (Player player in _players)
        {
            MovePlayerOntoStartingTitle(player, screenLoadDuration);
        }

        yield return new WaitForSeconds(screenLoadDuration);

        if (_dialogueSystem.HasDialogue())
        {
            _dialogueSystem.StartDialogue();
            _dialogueSystem.OnDialogueEnd += WaitUntilDialogueEnds;
            UpdateGameState(GameState.Dialogue);
            void WaitUntilDialogueEnds()
            {
                ToggleIsPlaying(true);
                _dialogueSystem.OnDialogueEnd -= WaitUntilDialogueEnds;
            }
        }
        else
        {
            ToggleIsPlaying(true);
        }
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
            ToggleIsPlaying(true);
        }
        else if (_currentGameState == GameState.Playing)
        {
            ToggleIsPlaying(false);
        }
    }

    public void RestartGame()
    {
        if (_currentGameState == GameState.Win || _currentGameState == GameState.GameOver)
        {
            GlobalGameStateManager.Instance.RestartGameScene();
        }
    }

    void UpdateGameState(GameState gameState) 
    {
        _currentGameState = gameState;

        switch (gameState) 
        {
            case GameState.Playing:
                StartNextTick();
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
            hit.DestroyObject();
        }

    }

    void PlayerCollision(Player player)
    {
        bool canPlayerDie = player.OnHit();
        if (canPlayerDie)
        {
            player.OnDeath();
            _playerLives--;
            OnPlayerDeath?.Invoke(player, _playerLives);

            if (_playerLives > 0)
            {
                StartCoroutine(ResetScreen());
            }
            else
            {
                ClearAllPreviews();
                UpdateGameState(GameState.GameOver);
                StartCoroutine(ResetGame(TickDuration * 3)); //TODO: have the ability to put in more quaters to prevent this
            }
        }
    }

    IEnumerator ResetScreen()
    {
        ToggleIsPlaying(false, GameState.Transition);
        ClearAllPreviews();

        yield return new WaitForSeconds(_tickDuration * 2);

        _effectsSystem.PerformEffect(EffectType.DigitalGlitchIntensity, .5f);
        _effectsSystem.PerformEffect(EffectType.HorizontalShake, .125f);
        _effectsSystem.PerformEffect(EffectType.ScanLineJitter, .25f);

        yield return new WaitForSeconds(_tickDuration / 2);

        _playerFinishedWithScreen = 0;
        _ticksSinceScreenStart = 0;
        _spawnSystem.ClearObjects();
        _screenSystem.ResetScreenGridObjects(_spawnSystem, _gridSystem);

        foreach (Player player in _players)
        {
            player.OnSpawn();
            MovePlayerOntoStartingTitle(player, _tickDuration);
        }       
        
        _effectsSystem.PerformEffect(EffectType.DigitalGlitchIntensity, 0);
        _effectsSystem.PerformEffect(EffectType.ScanLineJitter, 0);
        _effectsSystem.PerformEffect(EffectType.HorizontalShake, 0);
        yield return new WaitForSeconds(_tickDuration);
        
        ToggleIsPlaying(true);
    }

    IEnumerator ResetGame(float delayUntilReset)
    { 
        yield return new WaitForSeconds(delayUntilReset);
        GlobalGameStateManager.Instance.ResetGame();
    }

    public List<Player> GetAllCurrentPlayers()
    {
        return _players;
    }

    public int GetLastIndexOfScramble()
    { 
        return _lastIndexForScrambling;
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

            if (previousPreview.isCreated)
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
        var preview = _spawnSystem.SpawnObjectAtTile(previewableBase.gameObject, previewTile, previewableObject.transform.rotation);
        preview.name = $"Preview of {previewableObject}";
        preview.transform.localScale = previewableObject.GetPreviewScale();
        var previewImage = previewableObject.GetPreviewSprite();

        var renderer = preview.GetComponent<SpriteRenderer>();
        renderer.sprite = previewImage;
        renderer.color = previewableObject.GetPreviewColor();
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
        var spawnedMovable = _spawnSystem.SpawnObjectAtTile(movableToBeCreated.gameObject, previewableCreatingMovable.CurrentTile, previewableCreatingMovable.transform.rotation);
        var moveable = spawnedMovable.GetComponent<GridMovable>();
        moveable.SetupMoveable(this, _spawnSystem, previewTile);
        moveable.travelDirection = movingDirection;
        
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
        _previewActions.Add(newPreview);

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
        EndCurrentTick(_tickEndDuration);
        yield return new WaitForSeconds(_tickEndDuration);

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

            movingObject.TransitionToTile(preview.previewTile, tickEndDuration);
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

        _gridSystem.TryGetTileByCoordinates(targetCoordinates.x, targetCoordinates.y, out var tile);

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
    public Tile previewTile;
    public bool isCreated;
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
