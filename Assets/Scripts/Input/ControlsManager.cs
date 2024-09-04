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
    bool _playersSameShuffle = true;
    ScrambleType _scrambleType;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += CheckIfScramble;
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;

        _gameManager.EffectsSystem.OnScrambleAmountChanged += (int scrambleAmount) => _amountToScramble = scrambleAmount;
        _gameManager.EffectsSystem.OnScrambleTypeChanged += (ScrambleType newScrambleType) => _scrambleType = newScrambleType;
        _gameManager.EffectsSystem.OnTicksUntilScrambleChanged += (int tickAmount) => _ticksPerScramble = tickAmount;
        _gameManager.EffectsSystem.OnMultiplayerScrambleTypeChanged += (bool isSame) => _playersSameShuffle = isSame;
    }  

    void CheckIfScramble(float tickTime)
    {
        _ticksSinceLastScramble++;

        if (_ticksSinceLastScramble >= _ticksPerScramble)
        {
            _ticksSinceLastScramble = 0;
            UpdateShuffledValues();
        }
    }

    void UpdateShuffledValues()
    {
        var unshuffledActions = new List<PlayerAction>();

        foreach (var player in _players)
        {
            unshuffledActions.AddRange(player.GetPossibleActions());
        }

        var previousShuffle = new List<PlayerAction>();

        foreach (var player in _players) 
        {
            //TODO: Allow different players to get other player's actions
            var unshuffled = unshuffledActions.Where(x => x.playerActionPerformedOn == player).ToList();
            var amountToScramble = GetLastIndexForScrambleType(_scrambleType);

            List<PlayerAction> shuffledValues;

            if (amountToScramble == 0)
            {
                shuffledValues = unshuffled;
            }
            else
            {
                if (_playersSameShuffle && previousShuffle.Count != 0)
                {
                    shuffledValues = new List<PlayerAction>();
                    for (int index = 0; index < previousShuffle.Count; index++)
                    {
                        var previousShuffledAction = previousShuffle[index];
                        var actionWithSameInput = unshuffled.Find(x => x.inputValue == previousShuffledAction.inputValue);
                        shuffledValues.Add(actionWithSameInput);
                    }
                }
                else
                {
                    shuffledValues = ShuffleInputs(unshuffled, player.GetScrambledActions(), _amountToScramble, amountToScramble);
                }
            }

            var unShuffledInputs = unshuffled.Select(x => x.inputValue).ToList();

            var playerActions = new SerializedDictionary<InputValue, PlayerAction>();
            for (int index = 0; index < unShuffledInputs.Count; index++)
            {
                playerActions.Add(unShuffledInputs[index], shuffledValues[index]);
            }

            player.SetScrambledActions(playerActions);

            if (_playersSameShuffle)
            {
                previousShuffle = shuffledValues;
            }
        }
    }

    List<PlayerAction> ShuffleInputs(List<PlayerAction> unshuffledValues, List<PlayerAction> lastShuffle, int amountOfOptionsToScramble, int totalAmountOfScrambleOptions)
    {
        List<PlayerAction> shuffledValues = new List<PlayerAction>();

        //this is the same as long as amount scramble options does change
        var valuesToShuffle = unshuffledValues.Take(totalAmountOfScrambleOptions).ToList();

        if (lastShuffle.Count != 0)
        {
            var lastShuffledValues = lastShuffle.Take(totalAmountOfScrambleOptions).ToList();

            //if we're still shuffling the same values, refer back to the last shuffle as our baseline, so that we change the previous
            //Hashset taken from: https://stackoverflow.com/questions/1673347/linq-determine-if-two-sequences-contains-exactly-the-same-elements
            if (new HashSet<PlayerAction>(valuesToShuffle).SetEquals(lastShuffledValues))
            {
                valuesToShuffle = lastShuffledValues;
            }
        }       

        //make a list of random numbers that we can pull from 
        var listNumbers = new List<int>();
        listNumbers.AddRange(Enumerable.Range(0, valuesToShuffle.Count())
                           .OrderBy(i => _random.Next())
                           .Take(amountOfOptionsToScramble));

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

        shuffledValues.AddRange(valuesToShuffle);

        //add any remaining values found in the unshuffledValues to the end of the shuffled values list
        shuffledValues.AddRange(unshuffledValues
            .Skip(totalAmountOfScrambleOptions)
            .Take(unshuffledValues.Count - totalAmountOfScrambleOptions));        

        return shuffledValues;
    }

    private void OnPlayerJoined(Player player)
    {
        _players = _gameManager.GetAllPlayers();
    }

    private int GetLastIndexForScrambleType(ScrambleType type)
    { 
        switch (type) 
        {
            case ScrambleType.None:
            default:
                return 0;
            case ScrambleType.Movement: 
                return 4;
            case ScrambleType.All: 
                return 5;
        }
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

    public override readonly string ToString() =>
        $"Input Value: {inputValue}; Player: {playerActionPerformedOn.name};";
}
