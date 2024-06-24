using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;

public class GameManager : MonoBehaviour
{
    public List<GridCoordinate> _startingPlayerPositions;
    public int ticksUntilRespawn = 3;
    public int numberOfLives = 3;
    public PreviewableBase previewableBase;

    //GameManager Events
    public delegate void TickStart(float timeToTickEnd);
    public TickStart OnTickStart;

    public delegate void TickEnd(float timeToTickStart);
    public TickEnd OnTickEnd;

    public delegate void ScreenChange();
    public ScreenChange OnScreenChange;

    public delegate void GameStateChanged(GameState newState);
    public GameStateChanged OnGameStateChanged;

    public delegate void PlayerJoined(Player player, int numberOfLives);
    public PlayerJoined OnPlayerJoinedGame;

    public delegate void PlayerDeath(Player player, Tile playerSpawnTile, int ticksUntilSpawn, int livesLeft);
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
    CommandSystem _commandSystem;

    float _tickDuration = .5f;
    bool _isMovementAtInput = false;

    float _tickElapsed = 0f;
    bool _tickIsOccuring = false;
    int _ticksSinceScreenStart = 0;

    int _lastIndexForScrambling = 4;

    void Awake()
    {
        _gridSystem = GetComponent<GridSystem>();
        _spawnSystem = GetComponent<SpawnSystem>();
        _commandSystem = GetComponent<CommandSystem>();
        //all these grabbing from the parameters should be temp code, but who knows
        TestParametersHandler.Instance.OnParametersChanged += UpdateAmountToScramble;
    }

    private void UpdateAmountToScramble(TestParameters newParameters)
    {
        _tickDuration = newParameters.tickDuration;
        _isMovementAtInput = newParameters.doesMoveOnInput;

        if (newParameters.amountControlsScrambled == 5)
        {
            _lastIndexForScrambling = 5;
        }
        else
        {
            _lastIndexForScrambling = 4;
        }
    }

    void Start()
    {       
        foreach (IManager managerObjects in FindAllManagers())
        {
            managerObjects.InitManager(this);
        }
    }

    private List<IManager> FindAllManagers()
    {
        IEnumerable<IManager> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IManager>();

        return new List<IManager>(dataPersistenceObjects);
    }

    //TODO: Rework this so that joining is called in a different manner. 
    //Right now, we have a manager handling this joining, but we'll need to reconfigure it to be a button press
    //Will ask Sean about how joinging a dsecond player normally goes
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.gameObject.TryGetComponent(out Player newPlayer);

        if (newPlayer != null)
        {
            int playerId = _players.Count;
            newPlayer.InitPlayer(this, playerInput, playerId);
            newPlayer.transform.SetParent(transform);
            var startingTile = GetStartingTileForPlayer(playerId);
            SpawnPlayer(newPlayer, startingTile);            

            _players.Add(newPlayer);
            _playerLives.Add(numberOfLives);
            OnPlayerJoinedGame?.Invoke(newPlayer, numberOfLives);
        }

        if (_players.Count == 1)
        {
            OnScreenChange?.Invoke();
            UpdateGameState(GameState.Playing);
        }
    }

    Tile GetStartingTileForPlayer(int playerId)
    {
        var startingPosition = _startingPlayerPositions[playerId];

        //TODO: Add a default spawning position, if the one provided is no longer valid for some reason
        _gridSystem.TryGetTileByCoordinates(startingPosition.x, startingPosition.y, out var startingTile);
        return startingTile;
    }

    public void SpawnPlayer(Player player, Tile tile)
    {
        player.SetPosition(tile);
        player.OnSpawn();
    }

    public void MovePlayerOntoSpawn(Player player, Tile tile, float duration)
    {
        var offscreenPosition = _spawnSystem.GetOffscreenPosition(player.transform.up, tile.GetTilePosition(), true);
        player.TransitionToPosition(offscreenPosition, 0);
        player.TransitionToTile(tile, duration);
    }

    public void PlayerGainedCondition(Player player, Condition condition)
    { 
        OnPlayerConditionStart?.Invoke(player, condition);
    }

    public void PlayerLostCondition(Player player, Condition condition)
    {
        OnPlayerConditionEnd?.Invoke(player, condition);
    }

    public void PlayerTriggeredScreenChange(Player player)
    {
        //stop the tick loop
        UpdateGameState(GameState.Paused);
        //disable all players' controls
        _players.ForEach(x => x.SetInputStatus(false));

        //move player off screen
        var currentPos = player.CurrentTile.GetTilePosition();
        var offscreenPosition = _spawnSystem.GetOffscreenPosition(player.transform.up, currentPos, false);
        player.TransitionToPosition(offscreenPosition, _tickDuration);
        //lets everyone know that the screen has been finished        
    }

    public void ClearObjects()
    {
        //remove everything that was on the grid
        _spawnSystem.ClearObjects();
        OnScreenChange?.Invoke();
    }

    public IEnumerator ScreenAnimationChangeFinished()
    {
        //renable controls for players
        foreach (Player player in _players)
        {
            var startingTile = GetStartingTileForPlayer(player.PlayerId);
            MovePlayerOntoSpawn(player, startingTile, _tickDuration);
        }

        yield return new WaitForSeconds(_tickDuration);        

        foreach (Player player in _players)
        {
            player.SetInputStatus(true);
        }
        
        //renable game loop
        UpdateGameState(GameState.Playing);
    }

    public void SetScreenStarters(List<ScreenSpawns> screenStarters)
    {
        foreach (var spawn in screenStarters)
        {
            if (_gridSystem.TryGetTileByCoordinates(spawn.spawnCoordinates.x, spawn.spawnCoordinates.y, out var spawnPosition))
            {
                var rotation = _spawnSystem.GetRotationFromSpawnDirection(spawn.facingDirection);
                var spawnedObject = _spawnSystem.SpawnObjectAtTile(spawn.gridObject.gameObject, spawnPosition, rotation);

                if (spawnedObject.TryGetComponent<GridMovable>(out var movable))
                {
                    movable.SetupMoveable(this, _spawnSystem, spawnPosition);
                }
                else
                {
                    spawnedObject.GetComponent<GridObject>().SetupObject(this, _spawnSystem);
                }                
            }
        }
    }

    public void SetQueuedEnemies(SerializedDictionary<int, EnemySpawn[]> screenWaves)
    {
        foreach (var screenPair in screenWaves)
        {
            var wave = screenPair.Value;

            foreach (var spawn in wave)
            {
                _spawnSystem.QueueEnemyToSpawn(spawn, screenPair.Key);
            }
        }
    }

    public List<ScreenChangeTrigger> SetScreenTranistions(ScreenChangeTrigger baseTrigger, List<GridCoordinate> transitionGrids)
    {
        var screenTriggers = new List<ScreenChangeTrigger>();
        foreach (var transition in transitionGrids)
        {
            if (_gridSystem.TryGetTileByCoordinates(transition.x, transition.y, out var spawnPosition))
            {
                var spawnedObject = _spawnSystem.SpawnObjectAtTile(baseTrigger.gameObject, spawnPosition, baseTrigger.transform.rotation);
                spawnedObject.GetComponent<GridObject>().SetupObject(this, _spawnSystem);
                var screenTrigger = spawnedObject.GetComponent<ScreenChangeTrigger>();
                screenTriggers.Add(screenTrigger);
            }
        }

        return screenTriggers;
    }

    public void SendEnemyCommands(EnemyShip enemyShip, int commandId)
    {
        enemyShip.shipCommands = _commandSystem.commandBank[commandId].shipCommands;
    }

    void UpdateGameState(GameState gameState) 
    {
        _currentGameState = gameState;

        switch (gameState) 
        {
            case GameState.Playing:
                StartNextTick();
                break;
            case GameState.Paused:
            case GameState.GameOver: //this might run into a race condition with on tick end
                _tickIsOccuring = false;
                break;
        }

        OnGameStateChanged?.Invoke(_currentGameState);
    }

    public void HandleGridObjectCollision(GridObject attacking, GridObject hit) //Should this be here in this state? Feels like something the grid object itself should be in charge of
    {
        attacking.DestroyObject();
        
        if (hit.TryGetComponent<Player>(out var player))
        {
            bool canPlayerDie = player.OnHit();
            if (canPlayerDie)
            {
                player.OnDeath();
                int lives = _playerLives[player.PlayerId];
                lives--;
                _playerLives[player.PlayerId] = lives;

                if (_playerLives.All(x => x <= 0))
                {
                    UpdateGameState(GameState.GameOver);
                    return;
                }

                if (lives >= 0)
                {
                    var startingPosition = _startingPlayerPositions[player.PlayerId];
                    _gridSystem.TryGetTileByCoordinates(startingPosition.x, startingPosition.y, out var spawnTile);
                    _spawnSystem.QueuePlayerToSpawn(player, spawnTile, _ticksSinceScreenStart + ticksUntilRespawn);

                    OnPlayerDeath?.Invoke(player, spawnTile, ticksUntilRespawn, lives);
                }
                else
                { 
                    //TODO: Waiting for player to put in more money
                }
            }
            return;
        }

        hit.DestroyObject();
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

        if (_isMovementAtInput)
        {
            _tickElapsed = _tickDuration;
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
        if (_tickElapsed >= _tickDuration) 
        {
            StartCoroutine(SetupNewTick());
        }
    }

    IEnumerator SetupNewTick()
    {
        _tickIsOccuring = false;
        var tickEndDuration = _tickDuration / 4;
        EndCurrentTick(tickEndDuration);
        yield return new WaitForSeconds(tickEndDuration);

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
        
        OnTickEnd?.Invoke(tickEndDuration);
    }

    void StartNextTick()
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
        OnTickStart?.Invoke(_tickDuration);
        _tickIsOccuring = true;
        _ticksSinceScreenStart++;
        _tickElapsed = 0;
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
        float timeRemaining = _tickDuration - _tickElapsed;
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
    Cutscene,
    GameOver
}
