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
    [Range(2, 10)]
    public int ticksUntilRespawn = 3;
    public int numberOfLives = 3;
    public PreviewableBase previewableBase;

    //GameManager Events
    public delegate void LevelStart(int levelId);
    public LevelStart OnLevelStart;

    public delegate void LevelEnd(int ticksPassed);
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
    private List<int> _playerLives = new List<int>();

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

        CreatePlayerShip();

        if (GlobalGameState.Instance && GlobalGameState.Instance.PlayerCount == 2)
        {
            CreatePlayerShip();
        }

        StartCoroutine(SetupNextScreen(_screenSystem.GetScreensRemaining(), TickDuration, false));
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
        newPlayer.InitPlayer(this, shipInfos[playerId], playerId);
        newPlayer.transform.SetParent(transform);

        _players.Add(newPlayer);
        _playerLives.Add(numberOfLives);

        var startingTile = GetStartingTileForPlayer(_players.Count, playerId);

        SpawnPlayer(newPlayer, startingTile, false);
        OnPlayerJoinedGame?.Invoke(newPlayer, numberOfLives);
    }

    Tile GetStartingTileForPlayer(int playerAmount, int playerId)
    {
        var startingPosition = _startingPlayerPositions[playerAmount][playerId];

        //TODO: Add a default spawning position, if the one provided is no longer valid for some reason
        _gridSystem.TryGetTileByCoordinates(startingPosition.x, startingPosition.y, out var startingTile);
        return startingTile;
    }

    public void SpawnPlayer(Player player, Tile tile, bool moveOnScreen = true)
    {
        MovePreviewableToOffScreenRelativeToTile(player, tile, _tickEndDuration, moveOnScreen);
        player.OnSpawn();
    }

    public void MovePreviewableOffScreenToTile(Previewable preview, Tile tile, float duration)
    {
        var offscreenPosition = _spawnSystem.GetOffscreenPosition(preview.transform.up, tile.GetTilePosition(), false);
        preview.TransitionToPosition(offscreenPosition, duration);
    }

    public void MovePreviewableToOffScreenRelativeToTile(Previewable preview, Tile tile, float duration, bool moveOnScreen = true)
    {
        var offscreenPosition = _spawnSystem.GetOffscreenPosition(preview.transform.up, tile.GetTilePosition(), true);
        preview.TransitionToPosition(offscreenPosition, 0);

        if (moveOnScreen)
        {
            preview.TransitionToTile(tile, duration);
        }
    }

    public void PlayerGainedCondition(Player player, Condition condition)
    { 
        OnPlayerConditionStart?.Invoke(player, condition);
    }

    public void PlayerLostCondition(Player player, Condition condition)
    {
        OnPlayerConditionEnd?.Invoke(player, condition);
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
        ClearAllPreviews();
        UpdateGameState(GameState.Transition);
        EndScreen(TickDuration);
    }

    void EndScreen(float endingDuation)
    {
        //disable all players' controls
        _players.ForEach(player =>
        {
            player.SetInputStatus(false);
            //move player off screen
            var currentPos = player.CurrentTile.GetTilePosition();
            var offscreenPosition = _spawnSystem.GetOffscreenPosition(player.transform.up, currentPos, false);
            player.TransitionToPosition(offscreenPosition, endingDuation);
        });

        int screensRemainingInLevel = _screenSystem.GetScreensRemaining();
        if (screensRemainingInLevel <= 0)
        {
            OnLevelEnd?.Invoke(_ticksSinceLevelStart);
            UpdateGameState(GameState.Win);
        }
        else
        {
            StartCoroutine(SetupNextScreen(screensRemainingInLevel, TickDuration));
        }
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
        yield return new WaitForSeconds(screenLoadDuration);

        //move ships on screen
        foreach (Player player in _players)
        {
            var startingTile = GetStartingTileForPlayer(_players.Count, player.PlayerId);
            MovePreviewableToOffScreenRelativeToTile(player, startingTile, screenLoadDuration);
        }

        yield return new WaitForSeconds(screenLoadDuration);

        if (_dialogueSystem.HasDialogue())
        {
            _dialogueSystem.StartDialogue();
            _dialogueSystem.OnDialogueEnd += WaitUntilDialogueEnds;
            UpdateGameState(GameState.Dialogue);
            void WaitUntilDialogueEnds()
            {
                RenablePlaying();
                _dialogueSystem.OnDialogueEnd -= WaitUntilDialogueEnds;
            }
        }
        else
        {
            RenablePlaying();
        }
    }

    public bool IsInDialogue()
    { 
        return (_currentGameState == GameState.Dialogue);
    }

    public void PlayerAdvancedDialogue()
    { 
        _dialogueSystem.AdvanceDialoguePressed();
    }

    public void RenablePlaying()
    {
        foreach (Player player in _players)
        {
            player.SetInputStatus(true);
        }

        //renable game loop
        UpdateGameState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (_currentGameState == GameState.Paused)
        {
            UpdateGameState(GameState.Playing);
            TestParametersHandler.Instance.ToggleOptions();
        }
        else if (_currentGameState == GameState.Playing)
        {
            UpdateGameState(GameState.Paused);
            TestParametersHandler.Instance.ToggleOptions();
        }
    }

    public void RestartGame()
    {
        if (_currentGameState == GameState.Win || _currentGameState == GameState.GameOver)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
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
            int lives = _playerLives[player.PlayerId];
            lives--;
            _playerLives[player.PlayerId] = lives;
            OnPlayerDeath?.Invoke(player, lives);

            if (_playerLives.All(x => x <= 0))
            {
                UpdateGameState(GameState.GameOver);
                return;
            }

            if (lives > 0)
            {
                var spawnTile = GetStartingTileForPlayer(_players.Count, player.PlayerId);
                _spawnSystem.QueuePlayerToSpawn(player, spawnTile, _ticksSinceScreenStart + ticksUntilRespawn);
            }
            else
            {
                //TODO: Waiting for player to put in more money
            }
        }
    }

    public List<Player> GetAllCurrentPlayers()
    {
        return _players;
    }

    public int GetLastIndexOfScramble()
    { 
        return _lastIndexForScrambling; //TODO: Change this magic number to reflect gameplay progression
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

        if (_isMovementAtInput && _attemptedPlayerActions.Count == _players.Count) //wait until all the players have inputted before advancing
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
        float timeRemaining = TickDuration - _tickElapsed;
        if (timeRemaining < 0)
        { 
            timeRemaining = 0;
        }
        return timeRemaining;
    }

    public Tile GetByCoordinates(Vector2 targetCoordinates)
    { 
        _gridSystem.TryGetTileByCoordinates((int)targetCoordinates.x, (int)targetCoordinates.y, out var tile);
        return tile;
    }

    public Vector2 GetGridLimits()
    { 
        return new Vector2(_gridSystem.GetMaxWidthIndex(), _gridSystem.GetMaxHeightIndex());
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
