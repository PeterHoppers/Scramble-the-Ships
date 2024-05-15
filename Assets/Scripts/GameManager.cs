using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public List<Vector2> _startingPlayerPositions;
    public Bullet playerBullet;
    public delegate void TickStart(float timeToTickEnd);
    public TickStart OnTickStart;

    public delegate void TickEnd(float timeToTickStart);
    public TickEnd OnTickEnd;

    public delegate void PlayerDeath(int ticksUntilSpawn, Player player, Tile playerSpawnTile);
    public PlayerDeath OnPlayerDeath;

    Dictionary<Player, PreviewAction> _attemptedPlayerActions = new Dictionary<Player, PreviewAction>();
    List<PreviewAction> _previewActions = new List<PreviewAction>();
    private List<Player> _players = new List<Player>();

    GridSystem _gridSystem;

    float _tickDuration = .5f;
    bool _isMovementAtInput = false;

    float _tickElapsed = 0f;
    bool _tickIsOccuring = false;

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

            var startingPosition = _startingPlayerPositions[playerId];
            //TODO: Add a default spawning position, if the one provided is no longer valid for some reason
            _gridSystem.TryGetTileByCoordinates(startingPosition.x, startingPosition.y, out var startingTile);
            newPlayer.SetPosition(startingTile);

            _players.Add(newPlayer);
        }

        if (_players.Count == 1)
        {
            StartCoroutine(StartNewTick());
        }
    }

    public void DestoryPlayer(Player player, Previewable attackingObject) 
    {
        attackingObject.DestroyPreviewable();
        player.OnDeath();

        var startingPosition = _startingPlayerPositions[player.PlayerId];
        _gridSystem.TryGetTileByCoordinates(startingPosition.x, startingPosition.y, out var spawnTile);

        OnPlayerDeath?.Invoke(3, player, spawnTile);
    }

    public List<Player> GetAllCurrentPlayers()
    {
        return _players;
    }

    public void AttemptPlayerAction(Player playerSent, PlayerAction playerInputValue)
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

        Previewable playerPreview = _players[playerInputValue.playerId];
        var previewTile = ConvertInputIntoPosition(playerPreview.GetGridCoordinates(), playerInputValue.inputValue);

        if (previewTile == null || !previewTile.IsVisible)
        {
            return;
        }

        //we need to double check if a movement action we're taking is actually valid
        //so we'll need to consult the grid system if we can move there or not
        //addiotnally, if we're going to create a new object next turn
        //we'll need to handle that as well, rather than just moving
        PreviewAction newPreview;

        if (playerInputValue.inputValue != InputValue.Shoot)
        {
            newPreview = CreatePreviewAtPosition(playerPreview, previewTile);            
        }
        else
        {
            var bulletPreview = Instantiate(playerBullet, playerSent.GetCurrentPosition(), playerSent.transform.rotation, transform);
            bulletPreview.SetupBullet(this, previewTile);
            newPreview = CreatePreviewAtPosition(bulletPreview, previewTile);
            newPreview.isCreated = true;
        }

        _attemptedPlayerActions.Add(playerSent, newPreview);
        _previewActions.Add(newPreview);

        if (_isMovementAtInput)
        {
            _tickElapsed = _tickDuration;
        }
    }

    public PreviewAction CreatePreviewAtPosition(Previewable previewableObject, Tile previewTile, bool isMoving = true)
    {
        var previewImage = previewableObject.GetPreviewSprite();

        var preview = new GameObject($"Preview of {previewableObject}");
        preview.transform.position = previewTile.GetTilePosition();
        preview.transform.rotation = previewableObject.transform.rotation;
        preview.transform.SetParent(transform);
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

    public Tile AddPreviewAtPosition(Previewable previewObject, Tile currentTile, Vector2 previewDirection)
    {
        var possibleGridCoordinates = previewDirection + currentTile.GetTilePosition();
        var isPossibleTileSpace = _gridSystem.TryGetTileByCoordinates(possibleGridCoordinates.x, possibleGridCoordinates.y, out Tile tile);

        PreviewAction newPreview;
        if (!isPossibleTileSpace)
        {
            return null;           
        }

        newPreview = CreatePreviewAtPosition(previewObject, tile);
        newPreview.previewTile = tile;

        _previewActions.Add(newPreview);
        return tile;
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

    Tile ConvertInputIntoPosition(Vector2 targetCoordinates, InputValue input)
    {
        switch (input)
        {
            case InputValue.Up:
            case InputValue.Shoot:
                targetCoordinates += Vector2.up;
                break;
            case InputValue.Down:
                targetCoordinates += Vector2.down;
                break;
            case InputValue.Left:
                targetCoordinates += Vector2.left;
                break;
            case InputValue.Right:
                targetCoordinates += Vector2.right;
                break;
            default:
                break;
        }

        _gridSystem.TryGetTileByCoordinates((int)targetCoordinates.x, (int)targetCoordinates.y, out var tile);

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
}

public struct PreviewAction
{ 
    public Previewable sourcePreviewable;
    public Tile previewTile;
    public bool isCreated;
    public bool isNotMoving;
}
