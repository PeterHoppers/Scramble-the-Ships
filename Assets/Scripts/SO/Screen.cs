using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Screen", menuName = "Screen")]
public class Screen : ScriptableObject
{
    [SerializedDictionary("Tick #", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawn[]> waveInformation = new SerializedDictionary<int, EnemySpawn[]>();
}
