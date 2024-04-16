using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Spawn", menuName = "Enemy Spawn")]
public class EnemySpawnInfo : ScriptableObject
{
    public GameObject enemyObject;
    public Vector2 spawnCoordiantes;
}
