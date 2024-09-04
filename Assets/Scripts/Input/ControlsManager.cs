using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AYellowpaper.SerializedCollections;

public class ControlsManager : MonoBehaviour, IManager
{
    private List<Player> _players = new List<Player>();
    private GameManager _gameManager;

    private System.Random _random = new System.Random();

    int _amountToScramble = 0;
    int _percentChanceNotDefaultScrambleAmount = 0;
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
        _gameManager.EffectsSystem.OnScrambleVarianceChanged += (int scrambleVarience) => _percentChanceNotDefaultScrambleAmount = scrambleVarience;
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
            var lastIndexForScrambling = GetLastIndexForScrambleType(_scrambleType);
            var amountToScrambleWithVarience = AdjustScrambleAmountForVarience(_amountToScramble, _percentChanceNotDefaultScrambleAmount);

            List<PlayerAction> shuffledValues;
            var currentControlsForPlayer = player.GetScrambledActions();

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
                if (lastIndexForScrambling == 0 || amountToScrambleWithVarience == 0)
                {
                    if (_scrambleType == ScrambleType.None || currentControlsForPlayer.Count == 0)
                    {
                        shuffledValues = unshuffled;
                    }
                    else
                    {
                        shuffledValues = currentControlsForPlayer;
                    }
                }
                else
                {
                    shuffledValues = ShuffleInputs(unshuffled, currentControlsForPlayer, amountToScrambleWithVarience, lastIndexForScrambling);
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

        //you can't scramble less than 2 options
        if (amountOfOptionsToScramble < 2)
        {
            amountOfOptionsToScramble = 2;
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

    private int AdjustScrambleAmountForVarience(int defaultScrambleAmount, int percentageToChange)
    {
        int randomPercentage = _random.Next(0, 100);

        if (randomPercentage >= percentageToChange)
        { 
            return defaultScrambleAmount;
        }

        int randomScrambleAmount = _random.Next(0, defaultScrambleAmount);
        return randomScrambleAmount;        
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
