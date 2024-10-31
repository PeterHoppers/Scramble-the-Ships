using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour, IManager
{
    public GameObject gameUIHolder;
    public EnergyUI energyUI;
    public LevelProgressUI levelProgressUI;
    public PlayerStatusUI[] playerStatusUIs;
    public GameOverUI gameOverUI;
    public WinScreenUI winScreenUI;
    public TextMeshProUGUI levelNameUI;

    private float _revealEndScreenUIDelay = .5f;
    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnPlayerJoinedGame += OnPlayerJoined;
        _gameManager.OnPlayerLeaveGame += OnPlayerLeave;
        _gameManager.OnGameStateChanged += OnGameStateChanged;
        _gameManager.OnScreenChange += OnScreenChange;
        _gameManager.OnLevelEnd += OnLevelEnd;

        _gameManager.EnergySystem.OnEnergyChange += OnEnergyChange;
    }

    void Awake()
    {
        winScreenUI.gameObject.SetActive(false);
        gameOverUI.gameObject.SetActive(false);
        energyUI.gameObject.SetActive(false);
        UpdatePlayerStatusVisiblity(false);
        levelNameUI.text = GlobalGameStateManager.Instance.CurrentLevel.GetDisplayName(false);
    }

    void OnDisable()
    {
        if (_gameManager != null)
        {
            _gameManager.EnergySystem.OnEnergyChange -= OnEnergyChange;
        }
    }

    void OnLevelEnd(int levelNumber, int energyLeft, int continuesUsed)
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
            StartCoroutine(RevealLevelEndText(levelNumber, energyLeft, continuesUsed, _revealEndScreenUIDelay));
        }        
    }

    IEnumerator PlayCutscene(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        GlobalGameStateManager.Instance.PlayCutscene();
    }

    IEnumerator RevealLevelEndText(int levelNumber, int energyLeft, int continuesUsed, float waitDuration) 
    {
        yield return new WaitForSeconds(waitDuration);
        winScreenUI.gameObject.SetActive(true);
        winScreenUI.SetLevelScore(levelNumber, energyLeft, continuesUsed);
    }

    void OnScreenChange(int screenLoaded, int totalScreens)
    {
        levelProgressUI.SetupScreenUI(totalScreens);
        levelProgressUI.ScreensRemaining = screenLoaded;

        if (!energyUI.isActiveAndEnabled)
        {
            energyUI.gameObject.SetActive(true);
        }

        if (!playerStatusUIs[0].isActiveAndEnabled)
        {
            UpdatePlayerStatusVisiblity(true);
        }
    }    

    void OnGameStateChanged(GameState newState)
    {
        var isGameOver = (newState == GameState.GameOver);
        gameOverUI.SetGameOverState(isGameOver);       
    }

    void OnPlayerJoined(Player player)
    {
        if (player.PlayerId >= playerStatusUIs.Length)
        {
            return;
        }
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.AddPlayerReference(player);
    }

    void OnPlayerLeave(Player player)
    {
        if (player.PlayerId >= playerStatusUIs.Length)
        {
            return;
        }
        var playerStatus = playerStatusUIs[player.PlayerId];
        playerStatus.RemovePlayerReference(player);
    }

    void OnEnergyChange(int currentEnergy, int maxEnergy)
    {
        energyUI.SetEnergy(currentEnergy, maxEnergy);
    }   

    void UpdatePlayerStatusVisiblity(bool isVisible)
    {
        foreach (var ui in playerStatusUIs)
        {
            ui.gameObject.SetActive(isVisible);
        }
    }
}
