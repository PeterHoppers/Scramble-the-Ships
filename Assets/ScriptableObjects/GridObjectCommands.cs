using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Commands", menuName = "Commands")]
public class GridObjectCommands : ScriptableObject
{    
    [SerializedDictionary("Tick #", "Input to Perform")]
    public SerializedDictionary<int, InputValue> commands; 
    [Tooltip("What number tick that commands restart. If they should only happen once, keep at 0.")]
    public int commandsLoopAtTick = 0;
}
