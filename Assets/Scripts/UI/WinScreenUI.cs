using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("UI Configuration")]
    public TextMeshProUGUI previousScoreText;
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI energyScoreText;
    public TextMeshProUGUI continueText;
    public TextMeshProUGUI continuesScoreText;
    public TextMeshProUGUI totalScoreText;  

    [Space]
    [SerializeField]
    private AudioClip _victoryJingle;

    [Space]
    [SerializeField]
    private int _secondsUntilAutoContinue;
    private int _currentScore;

    [Space]
    [SerializeField]
    private ScoreManager _scoreManager;

    public void SetLevelScore(int energyLeft, int continuesUsed)
    {
        var _scoreConfiguration = _scoreManager.scoreConfiguration;
        var _energyValue = _scoreConfiguration.pointsPerEnergy;
        var _continueLossValue = _scoreConfiguration.pointsPerContinue;
        var _levelValue = _scoreConfiguration.pointsPerLevel;

        int previousScore = _scoreManager.CurrentScore;
        int energyScore = _energyValue * energyLeft;
        int continueScore = _continueLossValue * continuesUsed;
        int totalScore = previousScore + _levelValue + energyScore + continueScore;

        previousScoreText.text = previousScore.ToString();
        levelValueText.text = _levelValue.ToString();

        energyText.text = $"({energyLeft} x {_energyValue})";
        energyScoreText.text = energyScore.ToString();

        continueText.text = $"({continuesUsed} x {_continueLossValue})";
        continuesScoreText.text = continueScore.ToString();

        totalScoreText.text = totalScore.ToString();
        _currentScore = totalScore; //we have to store this as a private variable because of setting up the onclick in the Unity UI

        var advanceButton = gameObject.GetComponentInChildren<Button>();
        EventSystem.current.SetSelectedGameObject(advanceButton.gameObject);

        GlobalAudioManager.Instance.TransitionSongs(_victoryJingle, .05f);
        StartCoroutine(AutoAdvanceLevel(_secondsUntilAutoContinue));
    }

    IEnumerator AutoAdvanceLevel(float waitDuration)
    { 
        yield return new WaitForSeconds(waitDuration);
        MoveOntoNextLevel();
    }

    public void MoveOntoNextLevel()
    {
        GlobalGameStateManager.Instance.CurrentScore = _currentScore;
        GlobalGameStateManager.Instance.NextLevel();
    }
}
