using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AYellowpaper.SerializedCollections;

public class GlobalGameStateManager : MonoBehaviour
{
    public static GlobalGameStateManager Instance { get; private set; }
    public int PlayerCount { get; set; }
    public int CreditCount { get; set; }

    public int CutsceneID { get; set; }

    [SerializeField]
    private List<Level> _levels = new List<Level>();

    [Header("Default Locations")]
    public List<GridCoordinate> defaultLocationForTransitionGrids = new List<GridCoordinate>();
    public SerializedDictionary<int, List<GridCoordinate>> startingPlayerPositions;

    const int TUTORIAL_INDEX = 0;
    int _activeLevelIndex = TUTORIAL_INDEX;
    LevelSceneSystem _levelSceneSystem;

    public delegate void StateChange(GlobalGameStateStatus newState);
    public StateChange OnStateChange;

    public delegate void CreditsChange(int creditAmount);
    public CreditsChange OnCreditsChange;

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

    public void PlayTutorial()
    {
        SetLevel(TUTORIAL_INDEX);
    }

    public void SkipTutorial()
    {
        PlayCutscene();
    }

    public void SetLevel(int level)
    {
        _activeLevelIndex = level;
        GlobalGameStateStatus = GlobalGameStateStatus.Game;
        _levelSceneSystem.LoadGameScene();
    }

    public void PlayCutscene()
    {
        GlobalGameStateStatus = GlobalGameStateStatus.Cutscene;
        _levelSceneSystem.LoadCutsceneScene();
    }

    public void StartNextCutscene()
    {
        CutsceneID++;
        PlayCutscene();
    }

    public void NextLevel()
    {
        _activeLevelIndex++;
        _levelSceneSystem.ReloadCurrentScene();
    }

    public void AdvanceFromCutsceneToGame()
    {
        _activeLevelIndex++;

        if (_activeLevelIndex < _levels.Count)
        {
            SetLevel(_activeLevelIndex);
        }
        else
        { 
            ResetGame();
        }
    }

    public void RestartGameScene()
    { 
        _levelSceneSystem.ReloadCurrentScene();
    }

    public void ResetGame()
    {
        _activeLevelIndex = 0;
        CutsceneID = 0;
        GlobalGameStateStatus = GlobalGameStateStatus.Preview;
        _levelSceneSystem.LoadPreviewScene();
    }

    public Level GetLevelInfo()
    {
        GlobalGameStateStatus = GlobalGameStateStatus.Game;
        return _levels[_activeLevelIndex];
    }

    public bool IsActiveLevelTutorial()
    {
        return (_activeLevelIndex == TUTORIAL_INDEX);
    }

    void CoinInserted()
    {
        CreditCount++;
        OnCreditsChange.Invoke(CreditCount);
    }

    public void ClearCredits()
    {
        CreditCount = 0;
        OnCreditsChange.Invoke(CreditCount);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha5) || Input.GetKeyUp(KeyCode.Keypad5) || Input.GetKeyUp(KeyCode.Alpha6) || Input.GetKeyUp(KeyCode.Keypad6))
        {
            CoinInserted();
        }
    }
}

public enum GlobalGameStateStatus
{ 
    Preview,
    LevelSelect,
    Cutscene,
    Game,
    GameOver
}
