using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class ScoreManager : MonoBehaviour, IManager
{
    public ScoreConfiguration scoreConfiguration;
    [Header("UI")]
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI nextScoreToBeatText;

    private int _currentScore;
    private int _scoreAtScreenStart;
    private int _pointsPerScreenCompletion;
    private int _pointsPerTileMoved;

    private ScreenSystem _screenSystem;
    private GameManager _manager;
    private List<ScreenChangeTrigger> _screenTransitions;
    private Dictionary<Player, int> _playerMaxSpacesAway = new Dictionary<Player, int>();
    private Dictionary<Player, int> _playerSpacesAway = new Dictionary<Player, int>();
    private List<ScoreInfo> _scoreInfos;
    
    private ScoreInfo _nextScoreToBeat;
    private int _nextScoreToBeatIndex;
    private int _scoreIndexOnScreenChange;

    public int CurrentScore
    { 
        get { return _currentScore; }
        private set
        {
            _currentScore = value;
            currentScoreText.text = PrintScore(_currentScore);
            UpdateNextHighScore(value);
        }
    }

    void Start()
    {
        _scoreInfos = GlobalGameStateManager.Instance.ScoreInfos;
        _scoreInfos = _scoreInfos.Where(x => x.playerCount == GlobalGameStateManager.Instance.PlayerCount).OrderBy(x => x.scoreAmount).ToList();
        CurrentScore = GlobalGameStateManager.Instance.CurrentScore;        

        _scoreAtScreenStart = CurrentScore;
        _pointsPerScreenCompletion = scoreConfiguration.pointsPerScreenCompletion;
        _pointsPerTileMoved = scoreConfiguration.pointsPerTileMoved;

        if (GlobalGameStateManager.Instance.IsAIPlaying)
        {
            currentScoreText.gameObject.SetActive(false);
            nextScoreToBeatText.gameObject.SetActive(false);
        }
    }

    public void InitManager(GameManager manager)
    {
        _manager = manager;
        _screenSystem = manager.GetComponent<ScreenSystem>();
        manager.OnScreenChange += OnScreenChange;
        manager.OnTickEnd += OnTickEnd;
        manager.OnPlayerJoinedGame += OnPlayerJoined;
        manager.OnScreenResetStart += OnScreenResetStart;
        manager.OnGameStateChanged += OnGameStateChanged;
        manager.OnLevelEnd += OnLevelEnd;
    }

    private void OnPlayerJoined(Player player)
    {
        if (player.TryGetComponent<ObstaclePlayer>(out var obstacle))
        {
            return;
        }

        _playerSpacesAway.Add(player, int.MaxValue);
    }

    private void OnScreenChange(int nextScreenIndex, int maxScreens)
    {
        _screenTransitions = _screenSystem.GetCurrentScreenTransitions();
        if (nextScreenIndex > 1)
        {
            CurrentScore += _pointsPerScreenCompletion;
        }

        _scoreAtScreenStart = CurrentScore;
        _scoreIndexOnScreenChange = _nextScoreToBeatIndex;
        ResetScreenProgress();        
    }

    private void OnScreenResetStart()
    {
        ResetScreenProgress();
        CurrentScore = _scoreAtScreenStart;
        UpdateNextScoreDisplay(_scoreIndexOnScreenChange);
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.GameOver)
        {
            _manager.OnSubmitScore(CurrentScore);
        }
    }

    private void OnTickEnd(float timeToTickStart, int nextTickNumber)
    {
        var pointsThroughMovement = 0;

        _manager.GetAllPlayers().ForEach(p =>
        {
            var currentMinSpacesAway = DistanceAwayFromEnd(p, _playerSpacesAway[p]);
            
            if (currentMinSpacesAway != _playerSpacesAway[p])
            {
                _playerSpacesAway[p] = currentMinSpacesAway;
            }

            pointsThroughMovement += (_playerMaxSpacesAway[p] - currentMinSpacesAway) * _pointsPerTileMoved;
        });

        if (pointsThroughMovement < 0)
        {
            pointsThroughMovement = 0;
        }

        CurrentScore = pointsThroughMovement + _scoreAtScreenStart;
    }

    int DistanceAwayFromEnd(Player player, int currentMinSpacesAway)
    {
        var playerCoordinates = player.CurrentTile.gridCoordinates;
        foreach (var screenTransition in _screenTransitions)
        {
            var screenTransitionVector = screenTransition.GetGridCoordinates();
            var yDistance = Math.Abs(screenTransitionVector.y - playerCoordinates.y);
            var xDistance = Math.Abs(screenTransitionVector.x - playerCoordinates.x);
            var totalDistance = Convert.ToInt32(yDistance + xDistance);

            if (totalDistance < currentMinSpacesAway)
            {
                currentMinSpacesAway = totalDistance;
            }
        }

        return currentMinSpacesAway;
    }    

    void ResetScreenProgress()
    {
        _manager.OnTickStart += OnFirstTickStart;
        void OnFirstTickStart(float _, int currentTickNumber)
        {
            _manager.OnTickStart -= OnFirstTickStart;
            _playerMaxSpacesAway.Clear();
            _playerSpacesAway.Clear();
            _manager.GetAllPlayers().ForEach(p =>
            {
                _playerSpacesAway.Add(p, int.MaxValue);
                _playerMaxSpacesAway.Add(p, DistanceAwayFromEnd(p, int.MaxValue));
            });
        }
    }

    private void UpdateNextHighScore(int currentScore)
    {
        if (_scoreInfos == null)
        {
            return;
        }

        if (_nextScoreToBeat.playerCount == 0)
        {
            UpdateNextScoreDisplay(0);
        }

        if (currentScore <= _nextScoreToBeat.scoreAmount)
        {
            return;
        }

        for (int scoreIndex = _nextScoreToBeatIndex; scoreIndex < _scoreInfos.Count; scoreIndex++) 
        { 
            var nextScore = _scoreInfos[scoreIndex];

            if (currentScore <= nextScore.scoreAmount)
            {                
                UpdateNextScoreDisplay(scoreIndex);
                break;
            }
        }
    }

    void UpdateNextScoreDisplay(int scoreIndex)
    {
        _nextScoreToBeatIndex = scoreIndex;
        _nextScoreToBeat = _scoreInfos[_nextScoreToBeatIndex];
        var ranking = _scoreInfos.Count - scoreIndex;

        nextScoreToBeatText.text = $"High Score #{ranking}: {PrintScore(_nextScoreToBeat.scoreAmount)}";
    }

    string PrintScore(int score)
    {
        return string.Format("{0:D5}", score);
    }

    private void OnLevelEnd(int levelNumber, int energyLeft, int continuesUsed)
    {
        currentScoreText.gameObject.SetActive(false);
    }

    public EndLevelScoreInfo CalcEndLevelScoreInfo(int levelNumber, int energyLeft, int continuesUsed)
    {
        var _energyValue = scoreConfiguration.pointsPerEnergy;
        var _continueLossValue = scoreConfiguration.pointsPerContinue;
        var _levelValue = scoreConfiguration.pointsPerLevel;

        int previousScore = CurrentScore;
        int levelScore = _levelValue * levelNumber;
        int energyScore = _energyValue * energyLeft;
        int continueScore = _continueLossValue * continuesUsed;
        int totalScore = previousScore + levelScore + energyScore + continueScore;

        CurrentScore = totalScore;
        _manager.OnSubmitScore(CurrentScore);

        return new EndLevelScoreInfo()
        {
            previousScore = previousScore,
            levelScore = levelScore,
            energyScore = energyScore,
            continueScore = continueScore,
            totalScore = totalScore,
        };
    }
}

public struct EndLevelScoreInfo
{
    public int previousScore;
    public int levelScore;
    public int energyScore;
    public int continueScore;
    public int totalScore;
}
