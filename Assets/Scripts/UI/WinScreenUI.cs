using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("Score Calculations")]
    [SerializeField]
    private int _levelValue; //should this be in charge of calcuating the score? Probably not
    [SerializeField]
    private int _energyValue;
    [SerializeField]
    private int _continueLossValue;

    [Header("UI Configuration")]
    public TextMeshProUGUI previousScoreText;
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI pointsPerEnergyText;
    public TextMeshProUGUI energyLeftText;
    public TextMeshProUGUI energyScoreText;
    public TextMeshProUGUI continueValueText;
    public TextMeshProUGUI continuesUsedText;
    public TextMeshProUGUI continuesScoreText;
    public TextMeshProUGUI totalScoreText;

    [Space]
    [SerializeField]
    private AudioClip _victoryJingle;

    [Space]
    [SerializeField]
    private int _secondsUntilAutoContinue;
    private int _currentScore;

    public void SetLevelScore(int energyLeft, int continuesUsed)
    {
        int previousScore = GlobalGameStateManager.Instance.CurrentScore;
        int energyScore = _energyValue * energyLeft;
        int continueScore = _continueLossValue * continuesUsed * - 1;
        int totalScore = previousScore + _levelValue + energyScore + continueScore;

        previousScoreText.text = previousScore.ToString();
        levelValueText.text = _levelValue.ToString();

        pointsPerEnergyText.text = _energyValue.ToString();
        energyLeftText.text = energyLeft.ToString();
        energyScoreText.text = energyScore.ToString();

        continueValueText.text = (_continueLossValue * -1).ToString();
        continuesUsedText.text = continuesUsed.ToString();
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
        GlobalGameStateManager.Instance.StartNextCutscene();
    }
}
