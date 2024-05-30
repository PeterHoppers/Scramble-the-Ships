using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using AYellowpaper.SerializedCollections;

public class ControlsManager : MonoBehaviour, IManager
{
    private List<Player> _players = new List<Player>();
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
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;
    }

    void CheckIfScramble(float tickTime)
    {
        _ticksSinceLastScramble++;
        _players = _gameManager.GetAllCurrentPlayers();

        if (_ticksSinceLastScramble >= _ticksPerScramble)
        {
            _ticksSinceLastScramble = 0;
            UpdateShuffledValues();
        }        

        foreach (var player in _players)
        {
            player.ClearSelected();
            player.AllowingInput = true;
        }
    }

    void UpdateShuffledValues()
    {
        var unshuffledActions = new List<PlayerAction>();

        foreach (var player in _players)
        {
            unshuffledActions.AddRange(player.GetPossibleAction());
        }

        foreach (var player in _players) 
        {
            //TODO: Allow different players to get other player's actions
            var unshuffled = unshuffledActions.Where(x => x.playerActionPerformedOn == player).ToList();
            
            var shuffledValues = ShuffleInputs(unshuffled, 4); //TODO: Change this magic number to reflect gameplay progression
            var unShuffledInputs = unshuffled.Select(x => x.inputValue).ToList();

            var playerActions = new SerializedDictionary<InputValue, PlayerAction>();
            for (int index = 0; index < unShuffledInputs.Count; index++)
            {
                playerActions.Add(unShuffledInputs[index], shuffledValues[index]);
            }

            player.SetScrambledActions(playerActions);
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

    private void OnTickEnd(float timeToTickStart)
    {
        foreach (var player in _players)
        {
            player.AllowingInput = false;
        }
    }

    private void OnPlayerJoined(Player player)
    {
        UpdateShuffledValues();
    }
}

public enum InputValue
{    
    Forward = 0,
    Backward = 1,
    Port = 2,
    Starboard = 3,
    Fire = 4
}

[System.Serializable]
public struct PlayerAction
{
    public Player playerActionPerformedOn;
    public InputValue inputValue;
    public Sprite actionUI;
}
