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

    [Header("UI Configuration")]
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI energyLeftText;
    public TextMeshProUGUI addedScoreText;
    public TextMeshProUGUI totalScoreText;

    [SerializeField]
    private int _ticksUntilAutoContinue;

    public void SetLevelScore(int energyLeft, float tickDuration)
    {
        int energyScore = _energyValue * energyLeft;
        int totalScore = _levelValue + energyScore;

        levelValueText.text = _levelValue.ToString();
        valueText.text = _energyValue.ToString();
        energyLeftText.text = energyLeft.ToString();
        addedScoreText.text = energyScore.ToString();
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
