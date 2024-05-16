using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Commands", menuName = "Enemy Commands")]
public class EnemyCommands : ScriptableObject
{
    [SerializedDictionary("Tick #", "Input to Perform")]
    public SerializedDictionary<int, InputValue> shipCommands;
}
