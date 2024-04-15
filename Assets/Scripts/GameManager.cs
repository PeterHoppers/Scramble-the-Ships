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
        //there should only be one of each player's actions being saved here
        if (_attemptedPlayerActions.TryGetValue(playerSent, out PlayerPreview playerPreview)) 
        {
            Destroy(playerPreview.previewGameObject); //TODO: Let's pool this to make it more efficent
            _attemptedPlayerActions.Remove(playerSent);
        }

        var newPreview = new PlayerPreview()
        {
            playerAction = playerInputValue,
        };        

        if (_isMovementAtInput)
        {
            _tickElapsed = _tickDuration;
        }
        else
        {
            var inputValue = playerInputValue.inputValue;
            //generate preview based upon the action type
            if (inputValue != InputValue.Shoot)
            {
                //get the direction that the player is going to go
                //get the position that that player is already add
                var previewPosition = ConvertInputIntoPosition(playerSent, inputValue);
                var playerImage = playerSent.GetComponentInChildren<SpriteRenderer>().sprite;

                var preview = Instantiate(new GameObject(), previewPosition, Quaternion.identity, transform);
                var renderer = preview.AddComponent<SpriteRenderer>();
                renderer.sprite = playerImage;
                renderer.color = new Color(.75f, .75f, .75f, .5f);

                newPreview.previewGameObject = preview;
                //create the position that it will be
            }
        }

        _attemptedPlayerActions.Add(playerSent, newPreview);
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

        foreach (var action in _attemptedPlayerActions)
        {
            var actionValue = action.Value;
            var player = action.Key;            

            var targetPosition = ConvertInputIntoPosition(player, actionValue.playerAction.inputValue);
            player.MovePlayer(targetPosition, tickEndDuration);
        }
        
        OnTickEnd?.Invoke(tickEndDuration);
        yield return new WaitForSeconds(tickEndDuration);

        foreach (var action in _attemptedPlayerActions)
        {           
            Destroy(action.Value.previewGameObject);
        }

        _attemptedPlayerActions.Clear();

        OnTickStart?.Invoke(_tickDuration);
        _tickIsOccuring = true;
        _tickElapsed = 0;
    }

    Vector2 ConvertInputIntoPosition(Player player, InputValue input)
    {
        Vector2 targetPosition = player.gameObject.transform.position;
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
