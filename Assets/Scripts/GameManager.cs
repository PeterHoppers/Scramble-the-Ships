using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void TickStart(float timeToTickEnd);
    public TickStart OnTickStart;

    public delegate void TickEnd(float timeToTickStart);
    public TickEnd OnTickEnd;

    Dictionary<Player, PlayerAction> _attemptedPlayerActions = new Dictionary<Player, PlayerAction>();
    Coroutine _onTickStartCoroutine;
    
    float _tickDuration = .5f;
    bool _isMovementAtInput = false;

    float _tickElapsed = 0f;
    bool _tickIsOccuring = false;

    private void UpdateAmountToScramble(TestParameters newParameters)
    {
        _tickDuration = newParameters.tickDuration;
        _isMovementAtInput = newParameters.doesMoveOnInput;
    }

    void Start()
    {
        //all these grabbing from the parameters should be temp code, but who knows
        TestParametersHandler.Instance.OnParametersChanged += UpdateAmountToScramble;
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

    public void AttemptPlayerAction(Player playerSent, PlayerAction playerInputValue)
    {
        //there should only be one of each player's actions being saved here
        if (_attemptedPlayerActions.TryGetValue(playerSent, out PlayerAction playerAction)) 
        { 
            _attemptedPlayerActions.Remove(playerSent);
        }

        _attemptedPlayerActions.Add(playerSent, playerInputValue);

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
        foreach (var action in _attemptedPlayerActions)
        {
            print($"Player {action.Value.playerId} for action {action.Value.inputValue}");
        }
        _attemptedPlayerActions.Clear();
        var tickEndDuration = _tickDuration / 4;
        OnTickEnd?.Invoke(tickEndDuration);
        yield return new WaitForSeconds(tickEndDuration);
        OnTickStart?.Invoke(_tickDuration);
        _tickIsOccuring = true;
        _tickElapsed = 0;
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
