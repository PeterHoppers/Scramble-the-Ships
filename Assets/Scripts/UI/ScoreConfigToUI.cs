using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreConfigToUI : MonoBehaviour
{
    [SerializeField]
    private ScoreConfiguration _scoreConfiguration;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI _gridPoints;

    [SerializeField]
    private TextMeshProUGUI _completedGrid;

    [SerializeField]
    private TextMeshProUGUI _completedSector;

    [SerializeField]
    private TextMeshProUGUI _energyAtEnd;

    [SerializeField]
    private TextMeshProUGUI _continuesUsed;


    private void Awake()
    {
        DisplayPoints(_scoreConfiguration.pointsPerTileMoved, _gridPoints);
        DisplayPoints(_scoreConfiguration.pointsPerScreenCompletion, _completedGrid);
        DisplayPoints(_scoreConfiguration.pointsPerLevel, _completedSector);
        DisplayPoints(_scoreConfiguration.pointsPerEnergy, _energyAtEnd, " per Energy");
        DisplayPoints(_scoreConfiguration.pointsPerContinue, _continuesUsed);
    }

    void DisplayPoints(int points, TextMeshProUGUI ui, string extraText = "")
    {
        ui.text = $"{points} points{extraText}";
    }
}
