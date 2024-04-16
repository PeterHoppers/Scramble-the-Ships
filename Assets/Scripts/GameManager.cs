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
            newPlayer.InitPlayer(this, playerInput, _players.Count);
            var startingPosition = _startingPlayerPositions[_players.Count];
            newPlayer.transform.position = _gridSystem.GetPositionByCoordinate((int)startingPosition.x - 1, (int)startingPosition.y - 1).GetTilePosition();
            _players.Add(newPlayer);
        }

        if (_players.Count == 1)
        {
            StartCoroutine(StartNewTick());
        }
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

            Destroy(previousPreview.previewGameObject); //look into using pooling instead

            if (previousPreview.isCreated)
            {
                Destroy(previousPreview.sourcePreviewable.gameObject);
            }
        }

        Previewable playerPreview = _players[playerInputValue.playerId];
        var previewPosition = ConvertInputIntoPosition(playerPreview.GetCurrentPosition(), playerInputValue.inputValue);

        //we need to double check if a movement action we're taking is actually valid
        //so we'll need to consult the grid system if we can move there or not
        //addiotnally, if we're going to create a new object next turn
        //we'll need to handle that as well, rather than just moving
        PreviewAction newPreview;

        if (playerInputValue.inputValue != InputValue.Shoot)
        {
            newPreview = CreatePreviewAtPosition(playerPreview, previewPosition);
        }
        else
        {
            var bulletPreview = Instantiate(playerBullet, playerSent.GetCurrentPosition(), playerSent.transform.rotation, transform);
            bulletPreview.SetupBullet(this);
            newPreview = CreatePreviewAtPosition(bulletPreview, previewPosition);
            newPreview.isCreated = true;
        }        

        _attemptedPlayerActions.Add(playerSent, newPreview);
        _previewActions.Add(newPreview);

        if (_isMovementAtInput)
        {
            _tickElapsed = _tickDuration;
        }
    }

    public PreviewAction CreatePreviewAtPosition(Previewable previewObject, Vector2 previewPosition)
    {
        var playerImage = previewObject.GetPreviewSprite();

        var preview = new GameObject($"Preview of {previewObject}");
        preview.transform.position = previewPosition;
        preview.transform.rotation = previewObject.transform.rotation;
        preview.transform.SetParent(transform);
        var renderer = preview.AddComponent<SpriteRenderer>();
        renderer.sprite = playerImage;
        renderer.color = previewObject.GetPreviewColor();

        return new PreviewAction()
        {
            previewGameObject = preview,
            sourcePreviewable = previewObject
        };    
    }

    public void AddPreviewAtPosition(Previewable previewObject, Vector2 previewPosition)
    {
        var newPreview = CreatePreviewAtPosition(previewObject, previewPosition);
        _previewActions.Add(newPreview);
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
            var targetPosition = preview.previewGameObject.transform.position;
            preview.sourcePreviewable.Move(targetPosition, tickEndDuration);
        }

        OnTickEnd?.Invoke(tickEndDuration);
        yield return new WaitForSeconds(tickEndDuration);

        foreach (var preview in _previewActions)
        {
            Destroy(preview.previewGameObject);
        }

        _previewActions.Clear();
        _attemptedPlayerActions.Clear();
        OnTickStart?.Invoke(_tickDuration);
        _tickIsOccuring = true;
        _tickElapsed = 0;
    }

    Vector2 ConvertInputIntoPosition(Vector2 targetPosition, InputValue input)
    {
        switch (input)
        {
            case InputValue.Up:
            case InputValue.Shoot:
                targetPosition += Vector2.up;
                break;
            case InputValue.Down:
                targetPosition += Vector2.down;
                break;
            case InputValue.Left:
                targetPosition += Vector2.left;
                break;
            case InputValue.Right:
                targetPosition += Vector2.right;
                break;
            default:
                break;
        }

        return targetPosition;
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
}

public struct PreviewAction
{ 
    public GameObject previewGameObject;
    public Previewable sourcePreviewable;
    public bool isCreated;
}
