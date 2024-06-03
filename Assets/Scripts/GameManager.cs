using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public List<Vector2> _startingPlayerPositions;
    public int ticksUntilRespawn = 3;
    public int numberOfLives = 3;
    public delegate void TickStart(float timeToTickEnd);
    public TickStart OnTickStart;

    public delegate void TickEnd(float timeToTickStart);
    public TickEnd OnTickEnd;

    public delegate void PlayerJoined(Player player, int numberOfLives);
    public PlayerJoined OnPlayerJoinedGame;

    public delegate void PlayerDeath(Player player, Tile playerSpawnTile, int ticksUntilSpawn, int livesLeft);
    public PlayerDeath OnPlayerDeath;

    public delegate void PlayerConditionStart(Player player, Condition condition);
    public PlayerConditionStart OnPlayerConditionStart;

    public delegate void PlayerConditionEnd(Player player, Condition condition);
    public PlayerConditionEnd OnPlayerConditionEnd;

    Dictionary<Player, PreviewAction> _attemptedPlayerActions = new Dictionary<Player, PreviewAction>();
    List<PreviewAction> _previewActions = new List<PreviewAction>();
    private List<Player> _players = new List<Player>();
    private List<int> _playerLives = new List<int>();

    GridSystem _gridSystem;

    float _tickDuration = .5f;
    bool _isMovementAtInput = false;

    float _tickElapsed = 0f;
    bool _tickIsOccuring = false;

    int _lastIndexForScrambling = 4;

    void Awake()
    {
        _gridSystem = GetComponent<GridSystem>();
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

            var startingPosition = _startingPlayerPositions[playerId];
            
            //TODO: Add a default spawning position, if the one provided is no longer valid for some reason
            _gridSystem.TryGetTileByCoordinates(startingPosition.x, startingPosition.y, out var startingTile);
            newPlayer.SetPosition(startingTile);

            _players.Add(newPlayer);
            _playerLives.Add(numberOfLives);
            OnPlayerJoinedGame?.Invoke(newPlayer, numberOfLives);
        }

        if (_players.Count == 1)
        {
            StartCoroutine(StartNewTick());
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

    public void PreviewablesCollided(Previewable attacking, Previewable hit)
    {
        attacking.DestroyPreviewable();
        
        if (hit.TryGetComponent<Player>(out var player))
        {
            bool canPlayerDie = player.OnHit();
            if (canPlayerDie)
            {
                player.OnDeath();
                int lives = _playerLives[player.PlayerId];
                lives--;

                if (lives < 0) 
                {
                    print("Game over!");
                    return;
                }                

                var startingPosition = _startingPlayerPositions[player.PlayerId];
                _gridSystem.TryGetTileByCoordinates(startingPosition.x, startingPosition.y, out var spawnTile);

                OnPlayerDeath?.Invoke(player, spawnTile, ticksUntilRespawn, lives);
            }
            return;
        }

        hit.DestroyPreviewable();
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

    public PreviewAction CreatePreviewOfPreviewableAtTile(Previewable previewableObject, Tile previewTile, bool isMoving = true)
    {
        var previewImage = previewableObject.GetPreviewSprite();

        var preview = new GameObject($"Preview of {previewableObject}");
        preview.transform.SetParent(transform);
        preview.transform.localPosition = previewTile.GetTilePosition();
        preview.transform.rotation = previewableObject.transform.rotation;
        var renderer = preview.AddComponent<SpriteRenderer>();
        renderer.sprite = previewImage;
        renderer.color = previewableObject.GetPreviewColor();
        previewableObject.previewObject = preview;

        return new PreviewAction()
        {
            sourcePreviewable = previewableObject,
            isNotMoving = !isMoving,
            previewTile = previewTile
        };    
    }

    public PreviewAction CreateMovablePreviewAtTile(GridMovable movableToBeCreated, Previewable previewableCreatingMovable, Tile previewTile, Vector2 movingDirection)
    {
        var bulletPreview = Instantiate(movableToBeCreated, transform);
        bulletPreview.transform.SetParent(transform);
        bulletPreview.transform.localPosition = previewableCreatingMovable.GetCurrentPosition();
        bulletPreview.transform.rotation = previewableCreatingMovable.transform.rotation;
        bulletPreview.SetupMoveable(this, previewTile);
        bulletPreview.travelDirection = movingDirection;
        var newPreview = CreatePreviewOfPreviewableAtTile(bulletPreview, previewTile);
        newPreview.isCreated = true;
        return newPreview;
    }

    public Tile AddPreviewAtPosition(Previewable previewObject, Tile currentTile, Vector2 previewDirection)
    {
        var possibleGridCoordinates = previewDirection + currentTile.GetTilePosition();
        var isPossibleTileSpace = _gridSystem.TryGetTileByCoordinates(possibleGridCoordinates.x, possibleGridCoordinates.y, out Tile tile);

        PreviewAction newPreview;
        if (!isPossibleTileSpace)
        {
            return null;           
        }

        newPreview = CreatePreviewOfPreviewableAtTile(previewObject, tile);
        newPreview.previewTile = tile;

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
        if (!_tickIsOccuring)
        {
            return;
        }

        _tickElapsed += Time.deltaTime;
        if (_tickElapsed >= _tickDuration) 
        {
            StartCoroutine(StartNewTick());
        }
    }

    IEnumerator StartNewTick()
    {
        _tickIsOccuring = false;
        var tickEndDuration = _tickDuration / 4;
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
        yield return new WaitForSeconds(tickEndDuration);

        foreach (var preview in _previewActions)
        {
            Destroy(preview.sourcePreviewable.previewObject);
        }

        _previewActions.Clear();
        _attemptedPlayerActions.Clear();
        OnTickStart?.Invoke(_tickDuration);
        _tickIsOccuring = true;
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
        return new Vector2(_gridSystem.gridWidth - 1, _gridSystem.gridHeight - 1);
    }
}

public struct PreviewAction
{ 
    public Previewable sourcePreviewable;
    public Tile previewTile;
    public bool isCreated;
    public bool isNotMoving;
}
