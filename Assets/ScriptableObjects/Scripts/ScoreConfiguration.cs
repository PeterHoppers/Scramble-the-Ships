using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Score Info", menuName = "Score Info")]
public class ScoreConfiguration : ScriptableObject
{
    [Header("During Level Calculations")]
    public int pointsPerScreenCompletion;
    public int pointsPerTileMoved;

    [Header("Level Finished Calculations")]
    public int pointsPerLevel;
    public int pointsPerEnergy;
    public int pointsPerContinue;

}
