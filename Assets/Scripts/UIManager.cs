using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour, IManager
{
    public TickDurationUI tickDurationUI;
    public PlayerStatusUI[] playerStatusUIs;
    public GameStateDisplay gameStateDisplay;
    public TextMeshProUGUI tickAmountDisplay;
    public TextMeshProUGUI screenRemainingDisplay;

    GameManager _gameManager;
    int _ticksPassed = 0;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += OnTickStart;
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;
        _gameManager.OnPlayerConditionStart += OnPlayerConditionStart;
        _gameManager.OnPlayerConditionEnd += OnPlayerConditionEnd;
        _gameManager.OnPlayerDeath += OnPlayerDeath;
        _gameManager.OnGameStateChanged += OnGameStateChanged;
        _gameManager.OnScreenChange += OnScreenChange;
    }

    void OnScreenChange(int screensRemaining)
    {
        screenRemainingDisplay.text = screensRemaining.ToString();
    }

    void OnTickStart(float duration)
    {
        tickDurationUI.TickDuration = duration;
        _ticksPassed++;
        tickAmountDisplay.text = _ticksPassed.ToString();
    }

    void OnGameStateChanged(GameState newState)
    {
        gameStateDisplay.UpdateStateDisplay(newState);
    }

    void OnPlayerJoined(Player player, int numberOfLives)
    {
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.AddPlayerReference(player, numberOfLives);
    }

    void OnPlayerDeath(Player player, int livesLeft)
    {
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.DiedPlayerReference(livesLeft);
    }

    void OnPlayerConditionStart(Player player, Condition condition)
    {
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.GainedCondition(condition);
    }

    void OnPlayerConditionEnd(Player player, Condition condition)
    {
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.LostCondition(condition);
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameManager != null) 
        { 
            tickDurationUI.UpdateTickRemaining(_gameManager.GetTimeRemainingInTick());
        }
    }
}
