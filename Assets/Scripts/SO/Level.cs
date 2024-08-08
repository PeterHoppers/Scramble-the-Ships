using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "New Level")]
public class Level : ScriptableObject
{
    public Screen[] levelScreens;
    //possibly add effects that happen at the start of a level
    //add option in here to list different screens depending on player count
}
