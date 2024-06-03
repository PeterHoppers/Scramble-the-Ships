using System;
using UnityEngine;

public class UIManager : MonoBehaviour, IManager
{
    public TickDurationUI tickDurationUI;
    public PlayerStatusUI[] playerStatusUIs;

    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += OnTickStart;
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;
        _gameManager.OnPlayerConditionStart += OnPlayerConditionStart;
        _gameManager.OnPlayerConditionEnd += OnPlayerConditionEnd;
        _gameManager.OnPlayerDeath += OnPlayerDeath;
    }

    void OnTickStart(float duration)
    {
        tickDurationUI.TickDuration = duration;
    }

    void OnPlayerJoined(Player player, int numberOfLives)
    {
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.AddPlayerReference(player, numberOfLives);
    }

    void OnPlayerDeath(Player player, Tile playerSpawnTile, int ticksUntilSpawn, int livesLeft)
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
