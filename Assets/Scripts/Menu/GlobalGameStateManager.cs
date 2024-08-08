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
    int _activeLevelIndex = 0;
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
        DontDestroyOnLoad(this.gameObject);
        _levelSceneSystem = GetComponent<LevelSceneSystem>();
    }

    public void SetStartingLevel(int level)
    {
        _activeLevelIndex = level;
        GlobalGameStateStatus = GlobalGameStateStatus.Game;
        _levelSceneSystem.LoadGameScene();
    }

    public Level GetLevelInfo()
    { 
        return _levels[_activeLevelIndex];
    }
}

public enum GlobalGameStateStatus
{ 
    Preview,
    LevelSelect,
    Game
}
