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

    int _amountScrambledOption = 0;
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

        _gameManager.EffectsSystem.OnScrambleAmountChanged += (int scrambleAmount) => _amountScrambledOption = scrambleAmount;
        _gameManager.EffectsSystem.OnMultiplayerScrambleTypeChanged += (bool isSame) => _playersSameShuffle = isSame;
        _gameManager.EffectsSystem.OnScrambleVarianceChanged += (int scrambleVarience) => _percentChanceNotDefaultScrambleAmount = scrambleVarience;
        _gameManager.EffectsSystem.OnGameInputProgressionChanged += OnGameInputProgressionChanged;

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
        var allPossibleActions = new List<PlayerAction>();

        foreach (var player in _players)
        {
            if (!player.IsActive())
            {
                continue;
            }

            allPossibleActions.AddRange(player.GetPossibleActions());           
        }

        var amountToScramble = AdjustScrambleAmountForVarience(_amountScrambledOption, _percentChanceNotDefaultScrambleAmount);

        if (!_doesScrambleOnNoInput)
        {
            var hasPlayerInputted = true;

            foreach (var player in _players)
            {
                if (hasPlayerInputted)
                {
                    hasPlayerInputted = player.HasActiveInput();
                }
            }

            if (!hasPlayerInputted && !isForcedToShuffle)
            {
                amountToScramble = 0;
            }
        }

        var lastIndexForScrambling = GetLastIndexForScrambleType(_scrambleType);
        var previousShuffle = new List<PlayerAction>();

        foreach (var player in _players)
        {
            List<PlayerAction> shuffledValues;            
            var possibleActions = GetPossibleActionsForPlayer(player, allPossibleActions, lastIndexForScrambling);
            if (possibleActions.Count == 0)
            {
                shuffledValues = new List<PlayerAction>();
            }
            else if (_playersSameShuffle && previousShuffle.Count == possibleActions.Count)
            {
                shuffledValues = new List<PlayerAction>();
                for (int index = 0; index < previousShuffle.Count; index++)
                {
                    var previousShuffledAction = previousShuffle[index];
                    var actionWithSameInput = possibleActions.Find(x => x.inputValue == previousShuffledAction.inputValue);
                    if (actionWithSameInput.playerActionPerformedOn != null)
                    {
                        shuffledValues.Add(actionWithSameInput);
                    }
                }
            }
            else
            {
                shuffledValues = ShuffleInputs(possibleActions, player.GetScrambledActions(), amountToScramble, lastIndexForScrambling);
            }

            var unShuffledInputs = player.GetButtonValues(possibleActions.Count);

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

    List<PlayerAction> GetPossibleActionsForPlayer(Player player, List<PlayerAction> possibleActions, int lastIndexForScrambling)
    {
        if (_scrambleType == GameInputProgression.DummyShipDefault)
        {
            if (player.PlayerId == 0)
            {
                return possibleActions.Take(lastIndexForScrambling).ToList();
            }
            else
            {
                var remainingActions = possibleActions.Count - lastIndexForScrambling;
                return possibleActions.Skip(lastIndexForScrambling).Take(remainingActions).ToList();
            }
        }
        else if (_scrambleType == GameInputProgression.CrossScrambleShooting)
        { 
            var playerMovementActions = possibleActions.Where(x => x.playerActionPerformedOn == player && x.inputValue != InputValue.Fire).ToList();
            var otherPlayerShootingAction = possibleActions.FirstOrDefault(x => x.playerActionPerformedOn != player && x.inputValue == InputValue.Fire);
            
            if (otherPlayerShootingAction.playerActionPerformedOn != null)
            {
                playerMovementActions.Add(otherPlayerShootingAction);                
            }
            else
            {
                var playerShootingAction = possibleActions.FirstOrDefault(x => x.playerActionPerformedOn == player && x.inputValue == InputValue.Fire);

                if (playerShootingAction.playerActionPerformedOn != null)
                {
                    playerMovementActions.Add(playerShootingAction);
                }
            }

            return playerMovementActions;
        }

        return possibleActions.Where(x => x.playerActionPerformedOn == player).ToList();
    }

    List<PlayerAction> ShuffleInputs(List<PlayerAction> unshuffledValues, List<PlayerAction> lastShuffle, int amountOfOptionsToScramble, int totalAmountOfScrambleOptions)
    {
        //if it is the first scramble, set their possible values to the default
        if (lastShuffle.Count == 0)
        { 
            return unshuffledValues;
        }

        //if we don't want any controls to be scrambled at all, use the defaults
        if (totalAmountOfScrambleOptions == 0) 
        {
            return unshuffledValues;
        }

        //if we don't want to scramble their current controls, either keep them the same (if the options of controls haven't changed) or go to the default if they have
        if (amountOfOptionsToScramble == 0) 
        {
            //Hashset taken from:https://stackoverflow.com/questions/1673347/linq-determine-if-two-sequences-contains-exactly-the-same-elements
            if (new HashSet<PlayerAction>(unshuffledValues).SetEquals(lastShuffle))
            {
                return lastShuffle;
            }
            else
            {
                return unshuffledValues;
            }
        }

        List<PlayerAction> shuffledValues = new List<PlayerAction>();

        //this is the same as long as amount scramble options does change
        var valuesToShuffle = unshuffledValues.Take(totalAmountOfScrambleOptions).ToList();

        if (lastShuffle.Count != 0)
        {
            var lastShuffledValues = lastShuffle.Take(totalAmountOfScrambleOptions).ToList();

            //if we're still shuffling the same values, refer back to the last shuffle as our baseline, so that our new values are for sure different from the previous shuffle   
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

    private void OnGameInputProgressionChanged(GameInputProgression scrambleType)
    {
        if (_scrambleType == scrambleType)
        {
            return;
        }

        _scrambleType = scrambleType;

        if (_gameManager.GetGameState() != GameState.Cutscene)
        {
            return;
        }

        _gameManager.OnTickStart += OnNextTickOfProgression;
        void OnNextTickOfProgression(float _, int currentTickNumber)
        {
            _gameManager.OnTickStart -= OnNextTickOfProgression;
            UpdateShuffledValues(true);
        }
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
            case GameInputProgression.CrossScrambleShooting:
                return 4;
            case GameInputProgression.ScrambledShooting:
            case GameInputProgression.Rotation:
            case GameInputProgression.DummyShipDefault:
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

        if (minScramble > defaultScrambleAmount)
        {
            return minScramble;
        }

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
