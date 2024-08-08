using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameStateManager : MonoBehaviour
{
    public static GlobalGameStateManager Instance { get; private set; }
    public int PlayerCount { get; set; }

    [SerializeField]
    private List<Level> _levels = new List<Level>();
    const int TUTORIAL_INDEX = 0;
    int _activeLevelIndex = TUTORIAL_INDEX;
    LevelSceneSystem _levelSceneSystem;

    public delegate void StateChange(GlobalGameStateStatus newState);
    public StateChange OnStateChange;

    GlobalGameStateStatus _globalGameStateStatus;
    public GlobalGameStateStatus GlobalGameStateStatus 
    { 
        get { return _globalGameStateStatus; }
        set 
        { 
            _globalGameStateStatus = value;
            OnStateChange?.Invoke(value);
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Found more than one Global Game State in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _levelSceneSystem = GetComponent<LevelSceneSystem>();
    }

    public void SetStartingLevel(int level)
    {
        _activeLevelIndex = level;
        GlobalGameStateStatus = GlobalGameStateStatus.Game;
        _levelSceneSystem.LoadGameScene();
    }

    public void NextLevel()
    {
        _activeLevelIndex++;
        _levelSceneSystem.ReloadCurrentScene();
    }

    public void RestartGameScene()
    { 
        _levelSceneSystem.ReloadCurrentScene();
    }

    public Level GetLevelInfo()
    { 
        return _levels[_activeLevelIndex];
    }

    public bool IsActiveLevelTutorial()
    {
        return (_activeLevelIndex == TUTORIAL_INDEX);
    }
}

public enum GlobalGameStateStatus
{ 
    Preview,
    LevelSelect,
    Game
}
