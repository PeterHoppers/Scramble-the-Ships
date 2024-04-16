using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public List<Vector2> _startingPlayerPositions;
    public delegate void TickStart(float timeToTickEnd);
    public TickStart OnTickStart;

    public delegate void TickEnd(float timeToTickStart);
    public TickEnd OnTickEnd;

    Dictionary<Player, PlayerPreview> _attemptedPlayerActions = new Dictionary<Player, PlayerPreview>();
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

        _tickIsOccuring = true;
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
            newPlayer.transform.position = _gridSystem.GetPositionByCoordinate((int)startingPosition.x - 1, (int)startingPosition.y - 1);
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
        //we need to double check if a movement action we're taking is actually valid
        //so we'll need to consult the grid system if we can move there or not
        //addiotnally, if we're going to create a new object next turn
        //we'll need to handle that as well, rather than just moving
        AddPreview(_players[playerInputValue.playerId], playerInputValue.inputValue);
    }

    public void AddPreview(Previewable previewObject, InputValue inputValue)
    {
        var currentPreviewIndex = _previewActions.FindIndex(x => x.sourceGameObject == previewObject);
        if (currentPreviewIndex != - 1)
        { 
            var previousPreview = _previewActions[currentPreviewIndex];
            _previewActions.Remove(previousPreview);
            Destroy(previousPreview.previewGameObject); //look into using pooling instead
        }
               
        if (inputValue != InputValue.Shoot)
        {
            var previewPosition = ConvertInputIntoPosition(previewObject.GetCurrentPosition(), inputValue);
            var playerImage = previewObject.GetPreviewSprite();

            var preview = Instantiate(new GameObject(), previewPosition, Quaternion.identity, transform);
            var renderer = preview.AddComponent<SpriteRenderer>();
            renderer.sprite = playerImage;
            renderer.color = previewObject.GetPreviewColor();

            var newPreview = new PreviewAction()
            {
                previewGameObject = preview,
                sourceGameObject = previewObject
            };

            _previewActions.Add(newPreview);
        }

        if (_isMovementAtInput)
        {
            _tickElapsed = _tickDuration;
        }
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
            preview.sourceGameObject.Move(targetPosition, tickEndDuration);
        }

        OnTickEnd?.Invoke(tickEndDuration);
        yield return new WaitForSeconds(tickEndDuration);

        foreach (var preview in _previewActions)
        {
            Destroy(preview.previewGameObject);
        }

        _previewActions.Clear();
        OnTickStart?.Invoke(_tickDuration);
        _tickIsOccuring = true;
        _tickElapsed = 0;
    }
    Vector2 ConvertInputIntoPosition(Vector2 targetPosition, InputValue input)
    {
        switch (input)
        {
            case InputValue.Up:
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

public struct PlayerPreview
{
    public PlayerAction playerAction;
    public GameObject previewGameObject;
}

public struct PreviewAction
{
    public GameObject previewGameObject;
    public Previewable sourceGameObject;
    public bool isCreated;
}
