using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingManager : MonoBehaviour, IManager
{
    public static LoggingManager Instance { get; private set; }

    public bool IsLoggingInEditor = false;

    private GameManager _manager;
    private LoggingAdapter _loggingAdapter;
    private GlobalGameStateManager _globalGameStateManager;

    private GlobalGameStateStatus _previousState;
    private int _currentScreen = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        _globalGameStateManager = GlobalGameStateManager.Instance;
        _loggingAdapter = gameObject.AddComponent<LoggingAdapter>();
        _loggingAdapter.InitAdapter(IsLoggingInEditor);
    }

    void Start()
    {        
        PostToForm(new LoggingData()
        {
            playerEvent = LoggingEvents.OnGameLaunch
        });       
    }

    private void OnEnable()
    {
        _globalGameStateManager.OnCreditsChange += OnCreditChange;
        _globalGameStateManager.OnStateChange += OnStateChange;
    }
  
    public void InitManager(GameManager manager)
    {
        UnsubscribeFromManager();

        _manager = manager;
        _manager.OnLevelStart += OnLevelStart;
        _manager.OnLevelEnd += OnLevelEnd;
        _manager.OnPlayerDeath += OnPlayerDeath;
        _manager.OnScreenChange += OnScreenChange;
    }

    private void OnDestroy()
    {
        UnsubscribeFromManager();
    }

    void OnDisable()
    {
        UnsubscribeFromManager();

        _globalGameStateManager.OnCreditsChange -= OnCreditChange;
        _globalGameStateManager.OnStateChange -= OnStateChange;
    }

    private void UnsubscribeFromManager()
    {
        if (_manager == null)
        {
            return;
        }

        _manager.OnLevelStart -= OnLevelStart;
        _manager.OnLevelEnd -= OnLevelEnd;
        _manager.OnPlayerDeath -= OnPlayerDeath;
        _manager.OnScreenChange -= OnScreenChange;
    }

    private void OnCreditChange(int creditAmount)
    {
        var creditText = (creditAmount == int.MaxValue) ? "Freeplay" : creditAmount.ToString();

        PostToForm(new LoggingData()
        {
            playerEvent = LoggingEvents.OnCreditsChange,
            param1 = creditText
        });
    }

    private void OnStateChange(GlobalGameStateStatus newState)
    {
        if (newState == GlobalGameStateStatus.Game)
        {
            if (_previousState == GlobalGameStateStatus.GameOver)
            {
                PostToForm(new LoggingData()
                {
                    playerEvent = LoggingEvents.OnContinueUsed,
                    playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
                });
            }
            else if (_previousState == GlobalGameStateStatus.Preview)
            {
                PostToForm(new LoggingData()
                {
                    playerEvent = LoggingEvents.OnGameStart,
                    playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
                    param1 = GlobalGameStateManager.Instance.PlayerCount.ToString()
                });
            }
        }
        else if (newState == GlobalGameStateStatus.GameOver)
        {
            PostToForm(new LoggingData()
            {
                playerEvent = LoggingEvents.OnGameOver,
                playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
                param1 = GlobalGameStateManager.Instance.ActiveLevelIndex.ToString(),
                param2 = _currentScreen.ToString(),
            });
        }
        else if (newState == GlobalGameStateStatus.NameInput || newState == GlobalGameStateStatus.Preview)
        {
            if (_previousState == GlobalGameStateStatus.GameOver || _previousState == GlobalGameStateStatus.Cutscene)
            {
                PostToForm(new LoggingData()
                {
                    playerEvent = LoggingEvents.OnGameEnd,
                    playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
                    param1 = GlobalGameStateManager.Instance.CurrentScore.ToString(),
                });
            }
        }

        _previousState = newState;
    }

    private void OnScreenChange(int nextScreenIndex, int maxScreens)
    {
        _currentScreen = nextScreenIndex;
        var currentEnergy = _manager.GetComponent<EnergySystem>().CurrentEnergy;
        PostToForm(new LoggingData()
        {
            playerEvent = LoggingEvents.OnScreenStart,
            playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
            param1 = GlobalGameStateManager.Instance.ActiveLevelIndex.ToString(),
            param2 = _currentScreen.ToString(),
            param3 = currentEnergy.ToString(),
        });
    }

    private void OnPlayerDeath(Player player)
    {
        PostToForm(new LoggingData()
        {
            playerEvent = LoggingEvents.OnPlayerDied,
            playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
            param1 = GlobalGameStateManager.Instance.ActiveLevelIndex.ToString(),
            param2 = _currentScreen.ToString(),
        });
    }

    private void OnLevelEnd(int energyLeft, int continuesUsed)
    {
        PostToForm(new LoggingData()
        {
            playerEvent = LoggingEvents.OnLevelEnd,
            playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
            param1 = GlobalGameStateManager.Instance.ActiveLevelIndex.ToString(),
            param2 = GlobalGameStateManager.Instance.CurrentScore.ToString(),
            param3 = energyLeft.ToString(),
        });
    }

    private void OnLevelStart(int levelId)
    {
        PostToForm(new LoggingData()
        {
            playerEvent = LoggingEvents.OnLevelStart,
            playerGuid = GlobalGameStateManager.Instance.CurrentPlayingGUID,
            param1 = levelId.ToString()
        });
    }

    private void PostToForm(LoggingData data)
    {
        if (GlobalGameStateManager.Instance.IsAIPlaying)
        {
            return;
        }

        if (_loggingAdapter == null)
        {
            _loggingAdapter = gameObject.AddComponent<LoggingAdapter>();
            _loggingAdapter.InitAdapter(IsLoggingInEditor);
        }

        _loggingAdapter.PostLog(data);
    }
}

public struct LoggingData
{
    public Guid? playerGuid;
    public LoggingEvents playerEvent;
    public string param1;
    public string param2;
    public string param3;
}

public enum LoggingEvents
{
    OnGameLaunch,
    OnCreditsChange,
    OnGameStart,
    OnLevelStart,
    OnScreenStart,
    OnPlayerDied,
    OnGameOver,
    OnContinueUsed,
    OnLevelEnd,
    OnGameEnd
}