using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour, IManager
{
    public ScoreConfiguration scoreConfiguration;
    [Header("UI")]
    public TextMeshProUGUI currentScoreText;

    private int _currentScore;
    private int _scoreAtScreenStart;
    private int _pointsPerScreenCompletion;
    private int _pointsPerTileMoved;

    private ScreenSystem _screenSystem;
    private GameManager _manager;
    private List<ScreenChangeTrigger> _screenTransitions;
    private Dictionary<Player, int> _playerMaxSpacesAway = new Dictionary<Player, int>();
    private Dictionary<Player, int> _playerSpacesAway = new Dictionary<Player, int>();

    public int CurrentScore
    { 
        get { return _currentScore; }
        private set
        {
            _currentScore = value;
            currentScoreText.text = string.Format("{0:D5}", _currentScore);
        }
    }

    void Start()
    {
        CurrentScore = GlobalGameStateManager.Instance.CurrentScore;
        _scoreAtScreenStart = CurrentScore;
        _pointsPerScreenCompletion = scoreConfiguration.pointsPerScreenCompletion;
        _pointsPerTileMoved = scoreConfiguration.pointsPerTileMoved;
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
    }    

    private void OnPlayerJoined(Player player)
    {
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
        ResetScreenProgress();        
    }

    private void OnScreenResetStart()
    {
        ResetScreenProgress();
        CurrentScore = _scoreAtScreenStart;
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.GameOver)
        {
            GlobalGameStateManager.Instance.CurrentScore = CurrentScore;
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
        void OnFirstTickStart(float _)
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
}
