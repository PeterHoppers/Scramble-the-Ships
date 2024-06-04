using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class CommandSystem : MonoBehaviour
{
    [SerializedDictionary("Command Id", "Command SO")]
    public SerializedDictionary<int, EnemyCommands> commandBank = new SerializedDictionary<int, EnemyCommands>();
}
