using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class ControlsManager : MonoBehaviour
{
    private List<Player> _players = new List<Player>();
    private Dictionary<Player, InputValue> _lastPlayerInputs = new Dictionary<Player, InputValue>();    
    private Dictionary<Player, List<InputValue>> playerShuffledValues = new Dictionary<Player, List<InputValue>>();
    private InputValue[] unshuffledValues;

    private System.Random _random = new System.Random();

    private const int NUMBER_TO_SCRAMBLE = 3;

    private void Start()
    {
        unshuffledValues = Enum.GetValues(typeof(InputValue)).Cast<InputValue>().ToArray();
        UpdateShuffledValues();
    }

    //have this listen to the game maanger's onTickStart
    void UpdateShuffledValues()
    {
        foreach (var player in _players) 
        {
            playerShuffledValues[player] = ShuffleInputs(4);
        }
    }

    List<InputValue> ShuffleInputs(int amountScrambleOptions)
    {
        List<InputValue> shuffledValues = new List<InputValue>();
        var valuesToShuffle = unshuffledValues.Take(amountScrambleOptions).ToList();

        if (NUMBER_TO_SCRAMBLE == amountScrambleOptions)
        {
            valuesToShuffle = valuesToShuffle.OrderBy(_ => _random.Next()).ToList();
        }
        else if (NUMBER_TO_SCRAMBLE >= 2) //can't really shuffle less than 2 values
        {
            //make a list of random numbers that we can pull from 
            var listNumbers = new List<int>();
            listNumbers.AddRange(Enumerable.Range(0, valuesToShuffle.Count())
                               .OrderBy(i => _random.Next())
                               .Take(NUMBER_TO_SCRAMBLE));
            
            
            //create key value pairs based upon the input value that random number points to
            //and the next random number, which is where that input value is going to be assigned to
            var scrambledValues = new Dictionary<int, InputValue>();
            for (int index = 0; index < listNumbers.Count; index++)
            {
                InputValue inputValue = valuesToShuffle[listNumbers[index]];
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
            .Take(unshuffledValues.Length - amountScrambleOptions));        

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
            playerShuffledValues.Add(newPlayer, new List<InputValue>());
        }
    }

    public void TranslatePlayerInput(int id, Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            return;
        }

        var playerInputed = _players[id];
        InputValue simpleDirection = SimplifyDirection(direction);

        var playerInputValue = new PlayerInputValue()
        {
            player = playerInputed,
            inputValue = simpleDirection
        };

        //convert input direction into scrambled control direction
        var playerScrambledInput = TranslateScrambledInput(playerInputValue);

        SendInput(playerInputed, playerScrambledInput);
    }


    public void TranslatePlayerFireInput(int id, bool hasFired)
    {
        if (!hasFired)
        {
            return;
        }

        var playerIntereacted = _players[id];
        SendInput(playerIntereacted, InputValue.Shoot);
    }

    void SendInput(Player player, InputValue playerInputValue)
    {
        //check if this was the last recorded input we had for the player
        _lastPlayerInputs.TryGetValue(player, out var inputValue);

        //if it was not, update it and send the request to the game manager
        if (inputValue != playerInputValue)
        {
            print(playerInputValue);
            _lastPlayerInputs[player] = playerInputValue;
            //GameManager.PlayerAttemptToMove(Player player, Vector2 direction)            
        }
    }

    //rather than returning a Vector2, return a input type
    InputValue SimplifyDirection(Vector2 direction)
    {
        //convert input into a 2D directional

        if (direction.x > direction.y)
        {
            if (direction.x > 0)
            {
                return InputValue.Right;
            }
            else
            {
                return InputValue.Left;
            }
        }
        else
        {
            if (direction.y > 0)
            {
                return InputValue.Up;
            }
            else
            {
                return InputValue.Down;
            }
        }
    }

    //rather than taking in a Vector2, take in an input
    InputValue TranslateScrambledInput(PlayerInputValue playerInput)
    {
        int index = Array.IndexOf(unshuffledValues, playerInput.inputValue);
        return playerShuffledValues[playerInput.player][index];
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

public struct PlayerInputValue
{
    public Player player;
    public InputValue inputValue;
}
