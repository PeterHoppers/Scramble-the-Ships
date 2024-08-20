using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "New Level")]
public class Level : ScriptableObject
{
    public bool useOnePlayerForBoth;
    public Screen[] onePlayerLevelScreens;
    public Screen[] twoPlayerLevelScreens;
    //possibly add effects that happen at the start of a level    

    public Screen[] GetLevelScreens(int playerAmount)
    { 
        return (playerAmount == 1 || useOnePlayerForBoth) ? onePlayerLevelScreens : twoPlayerLevelScreens;
    }
}
