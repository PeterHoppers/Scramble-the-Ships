using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Spawn", menuName = "Enemy Spawn")]
public class EnemySpawnInfo : ScriptableObject
{
    public EnemySpawn[] enemySpawns;
}

[System.Serializable]
public struct EnemySpawn
{
    public GameObject enemyObject;
    public SpawnDirections spawnDirection;
    [Tooltip("The remaining coordinate, depending on the spawn direction, i.e top/bottom: x coordinate vs left/right: y coordinate")]
    public Coordinate otherCoordinate;
    public SpawnCommand spawnCommand;
}

[System.Serializable]
public enum SpawnDirections
{ 
    Top,
    Bottom,
    Left,
    Right
}

