using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
    [SerializeField]
    private int levelValue; //should this be in charge of calcuating the score? Probably not
    [SerializeField]
    private int tickValue;
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI tickValueText;
    public TextMeshProUGUI ticksPassedText;
    public TextMeshProUGUI tickScoreText;
    public TextMeshProUGUI totalScoreText;

    public void SetLevelScore(int ticksPassed)
    {
        int tickScore = tickValue * ticksPassed;
        int totalScore = levelValue + tickScore;

        levelValueText.text = levelValue.ToString();
        tickValueText.text = tickValue.ToString();
        ticksPassedText.text = ticksPassed.ToString();
        tickScoreText.text = tickScore.ToString();
        totalScoreText.text = totalScore.ToString();
    }
}
