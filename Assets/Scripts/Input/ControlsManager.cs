using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Net.NetworkInformation;

public class ControlsManager : MonoBehaviour, IManager
{
    private List<Player> _players = new List<Player>();
    private GameManager _gameManager;

    private System.Random _random = new System.Random();

    int _amountToScramble = 0;
    int _percentChanceNotDefaultScrambleAmount = 0;
    bool _playersSameShuffle = true;
    bool _doesScrambleOnNoInput = false;
    GameInputProgression _scrambleType;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickEnd += OnTickEnd;
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;
        _gameManager.OnPlayerLeaveGame += OnPlayerLeave;
        _gameManager.OnScreenChange += OnScreenChange;

        _gameManager.EffectsSystem.OnScrambleAmountChanged += (int scrambleAmount) => _amountToScramble = scrambleAmount;
        _gameManager.EffectsSystem.OnMultiplayerScrambleTypeChanged += (bool isSame) => _playersSameShuffle = isSame;
        _gameManager.EffectsSystem.OnScrambleVarianceChanged += (int scrambleVarience) => _percentChanceNotDefaultScrambleAmount = scrambleVarience;
        _gameManager.EffectsSystem.OnGameInputProgressionChanged += (GameInputProgression newScrambleType) => _scrambleType = newScrambleType;

        OptionsManager.Instance.OnParametersChanged += (GameSettingParameters gameSettings, SystemSettingParameters _) => _doesScrambleOnNoInput = gameSettings.doesScrambleOnNoInput;
    }

    void OnScreenChange(int nextScreenIndex, int maxScreens)
    {
        UpdateShuffledValues();
    }

    void OnTickEnd(float tickEndDuration, int _)
    {
        UpdateShuffledValues();
    }

    void UpdateShuffledValues(bool isForcedToShuffle = false)
    {
        var unshuffledActions = new List<PlayerAction>();
        var hasPlayerInputted = true;

        foreach (var player in _players)
        {
            if (!player.IsActive())
            {
                continue;
            }

            unshuffledActions.AddRange(player.GetPossibleActions());
            if (hasPlayerInputted)
            {                
                hasPlayerInputted = player.HasActiveInput();
            }
        }

        var previousShuffle = new List<PlayerAction>();
        var lastIndexForScrambling = GetLastIndexForScrambleType(_scrambleType);
        var amountToScrambleWithVarience = AdjustScrambleAmountForVarience(_amountToScramble, _percentChanceNotDefaultScrambleAmount);

        if (!hasPlayerInputted && !_doesScrambleOnNoInput)
        {
            if (!isForcedToShuffle)
            {
                amountToScrambleWithVarience = 0;
            }
        }

        foreach (var player in _players) 
        {            
            List<PlayerAction> unshuffled = new List<PlayerAction>();
            if (_scrambleType == GameInputProgression.DummyShip)
            {
                if (lastIndexForScrambling <= unshuffledActions.Count)
                {
                    unshuffled = unshuffledActions.Take(lastIndexForScrambling).ToList();
                }
                else
                {
                    if (unshuffledActions.Count == 0)
                    {                        
                        continue;
                    }
                    else
                    {
                        unshuffled = unshuffledActions.Take(unshuffledActions.Count).ToList();
                    }
                }
            }
            else
            {
                unshuffled = unshuffledActions.Where(x => x.playerActionPerformedOn == player).ToList();
            }

            unshuffledActions.RemoveAll(x => unshuffled.Contains(x));

            List<PlayerAction> shuffledValues;
            var currentControlsForPlayer = player.GetScrambledActions();

            if (_playersSameShuffle && previousShuffle.Count != 0 && _scrambleType != GameInputProgression.DummyShip)
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
                if (lastIndexForScrambling == 0 || amountToScrambleWithVarience == 0 || currentControlsForPlayer.Count == 0)
                {
                    if (_scrambleType == GameInputProgression.SimpleMovement || currentControlsForPlayer.Count == 0 || currentControlsForPlayer.Count != unshuffled.Count)
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

            var unShuffledInputs = player.GetButtonValues(unshuffled.Count);

            var playerActions = new SerializedDictionary<ButtonValue, PlayerAction>();
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
        player.OnPossibleInputsChanged += OnPlayerUpdatePossibleInputs;
        _players.Add(player);

        _gameManager.OnScreenChange += OnScreenChange;
        void OnScreenChange(int current_, int max_)
        {
            _gameManager.OnScreenChange -= OnScreenChange;
            UpdateShuffledValues();
        }
    }

    private void OnPlayerLeave(Player player)
    {
        player.OnPossibleInputsChanged -= OnPlayerUpdatePossibleInputs;
        _players.Remove(player);
        UpdateShuffledValues(true);
    }

    private void OnPlayerUpdatePossibleInputs(List<PlayerAction> possibleActions)
    {
        UpdateShuffledValues();
    }

    private void OnDisable()
    {
        foreach (var player in _players)
        {
            player.OnPossibleInputsChanged -= OnPlayerUpdatePossibleInputs;
        }
    }

    private int GetLastIndexForScrambleType(GameInputProgression type)
    { 
        switch (type) 
        {
            case GameInputProgression.SimpleMovement:
            default:
                return 0;
            case GameInputProgression.ScrambledMovement:
            case GameInputProgression.MoveAndShooting:
                return 4;
            case GameInputProgression.ScrambledShooting:
            case GameInputProgression.Rotation:
            case GameInputProgression.DummyShip:
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

        int minScramble = (_doesScrambleOnNoInput) ? 0 : 1;

        int randomScrambleAmount = _random.Next(minScramble, defaultScrambleAmount);
        return randomScrambleAmount;        
    }
}

public enum InputValue
{    
    Forward = 0,
    Backward = 1,
    Port = 2,
    Starboard = 3,
    Clockwise = 6,
    Counterclockwise = 7,
    Fire = 4,
    None = 5,
}

public enum ButtonValue
{ 
    Up,
    Down,
    Left,
    Right,
    Action
}

[System.Serializable]
public struct PlayerAction
{
    public Player playerActionPerformedOn;
    public InputValue inputValue;

    public override readonly string ToString() =>
        $"Input Value: {inputValue}; Player: {playerActionPerformedOn.name};";
}
