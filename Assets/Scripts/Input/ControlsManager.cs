using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using AYellowpaper.SerializedCollections;
using System.Collections;

public class ControlsManager : MonoBehaviour, IManager
{
    private List<Player> _players = new List<Player>();
    public List<PlayerAction> unshuffledValues = new List<PlayerAction>();
    private GameManager _gameManager;

    private System.Random _random = new System.Random();

    int _amountToScramble = 0;
    int _ticksPerScramble = 1;
    int _ticksSinceLastScramble = 0;

    private void Awake()
    {
        TestParametersHandler.Instance.OnParametersChanged += UpdateAmountToScramble;
    }

    private void UpdateAmountToScramble(TestParameters newParameters)
    {
        _amountToScramble = newParameters.amountControlsScrambled;
        _ticksPerScramble = newParameters.amountTickPerScramble;
    }

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += CheckIfScramble;
        _gameManager.OnTickEnd += OnTickEnd;
    }

    void CheckIfScramble(float tickTime)
    {
        _ticksSinceLastScramble++;

        if (_ticksSinceLastScramble >= _ticksPerScramble)
        {
            _ticksSinceLastScramble = 0;
            UpdateShuffledValues();
        }

        foreach (var player in _players)
        {
            player.AllowingInput = true;
        }
    }

    private void Start()
    {
        UpdateShuffledValues();
    }

    //have this listen to the game maanger's onTickStart
    void UpdateShuffledValues()
    {
        foreach (var player in _players) 
        {
            var unshuffled = unshuffledValues.Where(x => x.playerId == player.PlayerId).ToList();
            
            var shuffledValues = ShuffleInputs(unshuffled, 4);
            var unShuffledInputs = unshuffled.Select(x => x.inputValue).ToList();

            var playerActions = new SerializedDictionary<InputValue, PlayerAction>();
            for (int index = 0; index < unShuffledInputs.Count; index++)
            {
                playerActions.Add(unShuffledInputs[index], shuffledValues[index]);
            }

            player.SetPlayerActions(playerActions);
        }
    }

    List<PlayerAction> ShuffleInputs(List<PlayerAction> unshuffledValues, int amountScrambleOptions)
    {
        List<PlayerAction> shuffledValues = new List<PlayerAction>();
        var valuesToShuffle = unshuffledValues.Take(amountScrambleOptions).ToList();

        if (_amountToScramble == amountScrambleOptions)
        {
            valuesToShuffle = valuesToShuffle.OrderBy(_ => _random.Next()).ToList();
        }
        else if (_amountToScramble >= 2) //can't really shuffle less than 2 values
        {
            //make a list of random numbers that we can pull from 
            var listNumbers = new List<int>();
            listNumbers.AddRange(Enumerable.Range(0, valuesToShuffle.Count())
                               .OrderBy(i => _random.Next())
                               .Take(_amountToScramble));
            
            
            //create key value pairs based upon the input value that random number points to
            //and the next random number, which is where that input value is going to be assigned to
            var scrambledValues = new Dictionary<int, PlayerAction>();
            for (int index = 0; index < listNumbers.Count; index++)
            {
                PlayerAction inputValue = valuesToShuffle[listNumbers[index]];
                int nextIndex = index + 1;

                if (nextIndex >= listNumbers.Count)
                {
                    nextIndex = 0;
                }

                int targetIndex = listNumbers[nextIndex];
                scrambledValues.Add(targetIndex, inputValue);
            }

            foreach (var item in scrambledValues)
            {
                valuesToShuffle[item.Key] = item.Value;
            }
        }

        shuffledValues.AddRange(valuesToShuffle);

        //add any remaining values found in the unshuffledValues to the end of the shuffled values list
        shuffledValues.AddRange(unshuffledValues
            .Skip(amountScrambleOptions)
            .Take(unshuffledValues.Count - amountScrambleOptions));        

        return shuffledValues;
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
            _players.Add(newPlayer);
        }
    }

    private void OnTickEnd(float timeToTickStart)
    {
        foreach (var player in _players)
        {
            player.ClearSelected();
            player.AllowingInput = false;
        }
    }

    public void SendInput(Player playerSent, PlayerAction playerInputValue)
    {
        _gameManager.AttemptPlayerAction(playerSent, playerInputValue);
    }
}

public enum InputValue
{    
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    Shoot = 4
}

[System.Serializable]
public struct PlayerAction
{
    public int playerId;
    public InputValue inputValue;
    public Sprite actionUI;
}
