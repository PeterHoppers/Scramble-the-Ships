using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Screen", menuName = "Screen")]
public class Screen : ScriptableObject
{
    //TODO: Stuff that spawn on screen load
    [SerializedDictionary("Tick #", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawn[]> enemySpawnInformation = new SerializedDictionary<int, EnemySpawn[]>();
}

public struct ScreenSpawns
{
    public GridObject gridObject;
    public Vector2 spawnCoordinates;
}
