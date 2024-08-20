using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

[CreateAssetMenu(fileName = "New Level", menuName = "New Level")]
public class Level : ScriptableObject
{
    [Header("Screens")]
    public bool useOnePlayerForBoth;
    public Screen[] onePlayerLevelScreens;
    public Screen[] twoPlayerLevelScreens;

    public List<Effect> startingEffects;

    [Header("Level Defaults")]
    public bool overrideDefaultTransitionGrids = false;
    public List<GridCoordinate> transitionGrids = new List<GridCoordinate>();
    public bool overrideDefaultStartingPositions = false;
    public SerializedDictionary<int, List<GridCoordinate>> startingPlayerPositions;

    public Screen[] GetLevelScreens(int playerAmount)
    { 
        return (playerAmount == 1 || useOnePlayerForBoth) ? onePlayerLevelScreens : twoPlayerLevelScreens;
    }

    public List<GridCoordinate> GetStartingPlayerPositions(int playerAmount, Screen screen) 
    {
        if (screen.overrideDefaultStartingPositions)
        {
            return screen.startingPlayerPositions[playerAmount];
        }

        if (overrideDefaultStartingPositions)
        {
            return startingPlayerPositions[playerAmount];
        }  

        return GlobalGameStateManager.Instance.startingPlayerPositions[playerAmount];
    }

    public List<GridCoordinate> GetTransitionGridPositions(Screen screen)
    {
        if (screen.overrideDefaultTransitionGrids)
        {
            return screen.transitionGrids;
        }

        if (overrideDefaultTransitionGrids)
        {
            return transitionGrids;
        }

        return GlobalGameStateManager.Instance.defaultLocationForTransitionGrids;
    }
}
