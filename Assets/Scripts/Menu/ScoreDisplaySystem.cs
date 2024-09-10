using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreDisplaySystem : MonoBehaviour
{
    public PlayerAmount playerAmountInScore;
    public ScoreDisplay scoreDisplayPrefab;
    public GameObject scoreDisplayHolder;

    void Awake()
    {
        var scores = GlobalGameStateManager.Instance.ScoreInfos;
        var validScores = scores.Where(x => x.playerCount == (int)playerAmountInScore).OrderByDescending(x => x.scoreAmount);
        int rank = 1;

        foreach (var score in validScores) 
        { 
            var scoreDisplay = Instantiate(scoreDisplayPrefab, scoreDisplayHolder.transform);
            scoreDisplay.GetComponent<ScoreDisplay>().SetDisplay(rank, score);
            rank++;
        }
    }
}
