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
    public Vector2 spawnCoordiantes;
    public EnemyCommands commands;
}

[System.Serializable]
public enum SpawnDirections
{ 
    Top,
    Bottom,
    Left,
    Right
}

