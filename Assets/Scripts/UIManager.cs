using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour, IManager
{
    public TickDurationUI tickDurationUI;
    public EnergyUI energyUI;
    public PlayerStatusUI[] playerStatusUIs;
    public GameStateDisplay gameStateDisplay;
    public WinScreenUI winScreenUI;
    public GameObject gameUIHolder;
    public TextMeshProUGUI screenRemainingDisplay;
    public LivesUI livesUI;

    [Header("Images")]
    public List<Sprite> spritesForLives;

    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += OnTickStart;
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;
        _gameManager.OnPlayerConditionStart += OnPlayerConditionStart;
        _gameManager.OnPlayerConditionEnd += OnPlayerConditionEnd;
        _gameManager.OnGameStateChanged += OnGameStateChanged;
        _gameManager.OnScreenChange += OnScreenChange;
        _gameManager.OnLevelEnd += OnLevelEnd;

        _gameManager.EnergySystem.OnEnergyChange += OnEnergyChange;
    }

    void Awake()
    {
        winScreenUI.gameObject.SetActive(false);        
    }

    void OnLevelEnd(int energyLeft, int continuesUsed)
    {
        gameUIHolder.SetActive(false);
        StartCoroutine(RevealLevelEndText(energyLeft, continuesUsed));
    }

    IEnumerator RevealLevelEndText(int energyLeft, int continuesUsed) 
    {
        yield return new WaitForSeconds(.25f); //we could pass the tick length in here, but that might be overkill
        winScreenUI.gameObject.SetActive(true);
        winScreenUI.SetLevelScore(energyLeft, continuesUsed);
    }

    void OnScreenChange(int screensRemaining)
    {
        screenRemainingDisplay.text = screensRemaining.ToString();
    }

    void OnTickStart(float duration)
    {
        tickDurationUI.TickDuration = duration;
    }

    void OnGameStateChanged(GameState newState)
    {
        gameStateDisplay.UpdateStateDisplay(newState);
    }

    void OnPlayerJoined(Player player)
    {
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.AddPlayerReference(player);
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

    void OnEnergyChange(int currentEnergy, int maxEnergy)
    {
        energyUI.SetEnergy(currentEnergy, maxEnergy);
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
