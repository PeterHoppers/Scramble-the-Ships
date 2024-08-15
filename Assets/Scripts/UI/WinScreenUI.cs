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
    private int _tickValue;

    [Header("UI Configuration")]
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI tickValueText;
    public TextMeshProUGUI ticksPassedText;
    public TextMeshProUGUI tickScoreText;
    public TextMeshProUGUI totalScoreText;

    [SerializeField]
    private int _ticksUntilAutoContinue;

    public void SetLevelScore(int ticksPassed, float tickDuration)
    {
        int tickScore = _tickValue * ticksPassed;
        int totalScore = _levelValue + tickScore;

        levelValueText.text = _levelValue.ToString();
        tickValueText.text = _tickValue.ToString();
        ticksPassedText.text = ticksPassed.ToString();
        tickScoreText.text = tickScore.ToString();
        totalScoreText.text = totalScore.ToString();

        var advanceButton = gameObject.GetComponentInChildren<Button>().gameObject;

        EventSystem.current.SetSelectedGameObject(advanceButton);

        StartCoroutine(AutoadvanceLevel(tickDuration * _ticksUntilAutoContinue));
    }

    IEnumerator AutoadvanceLevel(float waitDuration)
    { 
        yield return new WaitForSeconds(waitDuration);
        MoveOntoNextLevel();
    }

    public void MoveOntoNextLevel()
    {
        GlobalGameStateManager.Instance.StartNextCutscene();
    }
}
