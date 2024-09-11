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
    public LevelProgressUI levelProgressUI;
    public PlayerStatusUI[] playerStatusUIs;
    public GameStateDisplay gameStateDisplay;
    public WinScreenUI winScreenUI;
    public GameObject gameUIHolder;

    private float _revealEndScreenUIDelay = .5f;
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
        if (_gameManager != null)
        {
            _gameManager.EnergySystem.OnEnergyChange -= OnEnergyChange;
        }

        gameUIHolder.SetActive(false);

        if (GlobalGameStateManager.Instance.IsActiveLevelTutorial())
        {
            StartCoroutine(PlayFirstCutscene(_revealEndScreenUIDelay));
        }
        else
        {
            StartCoroutine(RevealLevelEndText(energyLeft, continuesUsed, _revealEndScreenUIDelay));
        }        
    }

    IEnumerator PlayFirstCutscene(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        GlobalGameStateManager.Instance.PlayCutscene();
    }

    IEnumerator RevealLevelEndText(int energyLeft, int continuesUsed, float waitDuration) 
    {
        yield return new WaitForSeconds(waitDuration);
        winScreenUI.gameObject.SetActive(true);
        winScreenUI.SetLevelScore(energyLeft, continuesUsed);
    }

    void OnScreenChange(int screenLoaded, int totalScreens)
    {
        levelProgressUI.SetupScreenUI(totalScreens);
        levelProgressUI.ScreensRemaining = screenLoaded;
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
