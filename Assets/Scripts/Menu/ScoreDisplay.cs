using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI rankDisplay;
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI scoreDisplay;

    public void SetDisplay(int rank, ScoreInfo scoreInfo)
    {
        rankDisplay.text = $"{rank}.";
        nameDisplay.text = scoreInfo.displayName;
        scoreDisplay.text = scoreInfo.scoreAmount.ToString();
    }
}
