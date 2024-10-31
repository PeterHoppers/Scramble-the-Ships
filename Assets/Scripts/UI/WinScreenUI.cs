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
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelScoreText;
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

    [Space]
    [SerializeField]
    private ScoreManager _scoreManager;

    public void SetLevelScore(int levelNumber, int energyLeft, int continuesUsed)
    {
        var levelScoreInfo = _scoreManager.CalcEndLevelScoreInfo(levelNumber, energyLeft, continuesUsed);

        var _scoreConfiguration = _scoreManager.scoreConfiguration;
        var _energyValue = _scoreConfiguration.pointsPerEnergy;
        var _continueLossValue = _scoreConfiguration.pointsPerContinue;
        var _levelValue = _scoreConfiguration.pointsPerLevel;        

        previousScoreText.text = levelScoreInfo.previousScore.ToString();

        levelText.text = $"({levelNumber} x {_levelValue})";
        levelScoreText.text = levelScoreInfo.levelScore.ToString();

        energyText.text = $"({energyLeft} x {_energyValue})";
        energyScoreText.text = levelScoreInfo.energyScore.ToString();

        continueText.text = $"({continuesUsed} x {_continueLossValue})";
        continuesScoreText.text = levelScoreInfo.continueScore.ToString();

        totalScoreText.text = levelScoreInfo.totalScore.ToString();

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
        GlobalGameStateManager.Instance.NextLevel();
    }
}
