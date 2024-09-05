using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Screen", menuName = "Screen")]
public class Screen : ScriptableObject
{
    public List<ScreenSpawns> startingItems = new List<ScreenSpawns>();
    public bool overrideDefaultTransitionGrids = false;
    public List<GridCoordinate> transitionGrids = new List<GridCoordinate>();
    public bool overrideDefaultStartingPositions = false;
    public SerializedDictionary<int, List<GridCoordinate>> startingPlayerPositions;

    [Header("Action Items")]
    public bool useDifferentDialogueForBothPlayers = false;
    public Dialogue screenDialogue;
    public Dialogue twoPlayerDialogue;
    public List<Effect> effects;

    [SerializedDictionary("Tick #", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawn[]> enemySpawnInformation = new SerializedDictionary<int, EnemySpawn[]>();

    public int spawnsLoopAtTick;

    public Dialogue GetDialogue(int playerCount)
    {
        return (playerCount == 1 || !useDifferentDialogueForBothPlayers) ? screenDialogue : twoPlayerDialogue;
    }
}

[System.Serializable]
public struct ScreenSpawns
{
    public ObjectSpawnInfo spawnInfo;
    public GridCoordinate spawnCoordinates;
}

[System.Serializable]
public struct EnemySpawn
{
    public ObjectSpawnInfo spawnInfo;
    [Tooltip("The remaining coordinate, depending on the spawn direction, i.e top/bottom: x coordinate vs left/right: y coordinate")]
    public Coordinate otherCoordinate;
}

[System.Serializable]
public struct ObjectSpawnInfo
{
    public SpawnObject objectToSpawn;
    public SpawnDirections direction;
    [Range(1, 20)]
    public int groupingSize;
}
