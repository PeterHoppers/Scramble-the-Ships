using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Screen", menuName = "Screen")]
public class Screen : ScriptableObject
{
    [Header("Start Grid Coordinates")]
    public List<ScreenSpawns> startingItems = new List<ScreenSpawns>();
    public List<GridCoordinate> transitionGrids = new List<GridCoordinate>();

    [Header("Action Items")]
    public Dialogue screenDialogue;
    public List<Effect> effects;

    [SerializedDictionary("Tick #", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawn[]> enemySpawnInformation = new SerializedDictionary<int, EnemySpawn[]>();
}

[System.Serializable]
public struct ScreenSpawns
{
    public GridObject gridObject;
    public SpawnDirections facingDirection;
    public GridCoordinate spawnCoordinates;
    public SpawnCommand spawnCommand;
}
