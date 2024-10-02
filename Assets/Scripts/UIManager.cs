using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour, IManager
{
    public GameObject gameUIHolder;
    public TickDurationUI tickDurationUI;
    public EnergyUI energyUI;
    public LevelProgressUI levelProgressUI;
    public PlayerStatusUI[] playerStatusUIs;
    public GameOverUI gameOverUI;
    public WinScreenUI winScreenUI;
    public GameObject loadingUI;

    private float _revealEndScreenUIDelay = .5f;
    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += OnTickStart;
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;
        _gameManager.OnGameStateChanged += OnGameStateChanged;
        _gameManager.OnScreenChange += OnScreenChange;
        _gameManager.OnLevelEnd += OnLevelEnd;

        _gameManager.EnergySystem.OnEnergyChange += OnEnergyChange;
    }

    void Awake()
    {
        winScreenUI.gameObject.SetActive(false);
        gameOverUI.gameObject.SetActive(false);
        loadingUI.SetActive(true);
    }

    void OnDisable()
    {
        if (_gameManager != null)
        {
            _gameManager.EnergySystem.OnEnergyChange -= OnEnergyChange;
        }
    }

    void OnLevelEnd(int energyLeft, int continuesUsed)
    {
        if (_gameManager != null)
        {
            _gameManager.EnergySystem.OnEnergyChange -= OnEnergyChange;
        }

        gameUIHolder.SetActive(false);

        if (GlobalGameStateManager.Instance.ShouldSkipLevelEndPrompt())
        {
            StartCoroutine(PlayCutscene(_revealEndScreenUIDelay));
        }
        else
        {
            StartCoroutine(RevealLevelEndText(energyLeft, continuesUsed, _revealEndScreenUIDelay));
        }        
    }

    IEnumerator PlayCutscene(float waitDuration)
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
        var isGameOver = (newState == GameState.GameOver);
        gameOverUI.SetGameOverState(isGameOver);

        foreach (var ui in playerStatusUIs)
        { 
            ui.gameObject.SetActive(!isGameOver);
        }

        loadingUI.SetActive((newState == GameState.Waiting));
    }

    void OnPlayerJoined(Player player)
    {
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.AddPlayerReference(player);
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
