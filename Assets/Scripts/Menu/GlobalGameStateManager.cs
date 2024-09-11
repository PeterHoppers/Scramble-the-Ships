using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;

public class GlobalGameStateManager : MonoBehaviour, IDataPersistence
{
    public static GlobalGameStateManager Instance { get; private set; }
    public int PlayerCount { get; set; }
    public int CreditCount { get; set; }

    public int CutsceneID { get; set; }

    public int CurrentScore { get; set; }

    List<ScoreInfo> _scoreInfos;
    public List<ScoreInfo> ScoreInfos 
    {
        get { return _scoreInfos;}
    }

    [SerializeField]
    private List<Level> _levels = new List<Level>();

    [Header("Debugging")]
    [SerializeField]
    private bool isTwoPlayers;

    [Header("Default Locations")]
    public PlayerTransitionInfo defaultLocationForTransitionGrids;
    public SerializedDictionary<PlayerAmount, PlayerTransitionInfo> startingPlayerPositions;

    const int TUTORIAL_INDEX = 0;
    int _activeLevelIndex = TUTORIAL_INDEX;
    LevelSceneSystem _levelSceneSystem;
    CreditsSystem _creditsSystem;

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

        _creditsSystem = GetComponentInChildren<CreditsSystem>();
        _creditsSystem.OnCoinsChange += OnCoinsChange;

        if (isTwoPlayers)
        {
            PlayerCount = 2;
        }
        else
        {
            PlayerCount = 1;
        }

        UnityEngine.Screen.SetResolution(1920, 1080, true);
    }

    void Start()
    {
        GlobalGameStateStatus = GlobalGameStateStatus.Preview;
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

        if (HasHighScore(CurrentScore))
        {
            GlobalGameStateStatus = GlobalGameStateStatus.NameInput;
        }
        else
        {
            CurrentScore = 0;
            GlobalGameStateStatus = GlobalGameStateStatus.Preview;
        }
        
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

    private void OnCoinsChange(int coinsInserted, int creditsEarned)
    {
        if (CreditCount != creditsEarned)
        {
            CreditCount = creditsEarned;
            OnCreditsChange?.Invoke(CreditCount);
        }
    }

    public bool CanPlay(int playerAmount)
    {
        return (CreditCount >= playerAmount);
    }

    public void SetPlayerCount(int playerAmount)
    {
        PlayerCount = playerAmount;
        ConsumeCredits(playerAmount);        
    }

    public void ConsumeCredits(int creditAmount)
    { 
        _creditsSystem.RemoveCredits(creditAmount);
    }

    public void ClearCredits()
    {
        _creditsSystem.ClearCoins();
    }

    public void ClearScores()
    { 
        _scoreInfos = new ScoreSystem().GenerateDefaultScores();
        _levelSceneSystem.ReloadCurrentScene();
    }

    public void LoadData(SaveData data)
    {
        _scoreInfos = data.scores;
    }

    public void SaveData(SaveData data)
    {
        data.scores = _scoreInfos;
    }

    public void SubmitNameForScore(List<string> submittedName)
    { 
        var formattedName = new ScoreSystem().FormatMultipleNames(submittedName);
        AddScore(CurrentScore, formattedName);
        CurrentScore = 0;
        GlobalGameStateStatus = GlobalGameStateStatus.Preview;
    }

    private void AddScore(int score, string name)
    {
        var validScores = _scoreInfos.Where(x => x.playerCount == PlayerCount).OrderByDescending(x => x.scoreAmount).ToList();
        var scoreToDrop = validScores.Last();
        _scoreInfos.Remove(scoreToDrop);

        _scoreInfos.Add(new ScoreInfo()
        {
            scoreAmount = score,
            playerCount = PlayerCount,
            displayName = name
        });
    }

    private bool HasHighScore(int score)
    {
        var validScores = _scoreInfos.Where(x => x.playerCount == PlayerCount).ToList();
        return validScores.Any(x => x.scoreAmount < score);
    }
}

public enum GlobalGameStateStatus
{ 
    Preview,
    LevelSelect,
    Cutscene,
    Game,
    GameOver,
    NameInput
}

[System.Serializable]
public struct PlayerTransitionInfo
{
    public SpawnDirections direction;
    public List<GridCoordinate> positions;
}

public enum PlayerAmount
{ 
    OnePlayer = 1,
    TwoPlayer = 2,
}
