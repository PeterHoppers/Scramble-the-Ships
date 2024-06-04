using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Screen", menuName = "Screen")]
public class Screen : ScriptableObject
{
    public List<ScreenSpawns> startingItems = new List<ScreenSpawns>();

    [SerializedDictionary("Tick #", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawn[]> enemySpawnInformation = new SerializedDictionary<int, EnemySpawn[]>();
}

[System.Serializable]
public struct ScreenSpawns
{
    public GridObject gridObject;
    public Vector2 spawnCoordinates;
}
