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

    public AudioClip levelSong;

    [Header("Level Defaults")]
    public bool overrideDefaultTransitionGrids = false;
    public PlayerTransitionInfo transitionGrids;
    public bool overrideDefaultStartingPositions = false;
    public SerializedDictionary<PlayerAmount, PlayerTransitionInfo> startingPlayerPositions;

    public Screen[] GetLevelScreens(int playerAmount)
    { 
        return (playerAmount == 1 || useOnePlayerForBoth) ? onePlayerLevelScreens : twoPlayerLevelScreens;
    }

    public List<GridCoordinate> GetStartingPlayerPositions(int playerAmount, Screen screen) 
    {
        if (screen.overrideDefaultStartingPositions)
        {
            return GetStartingPositionsFromDictionary(screen.startingPlayerPositions, playerAmount);
        }

        if (overrideDefaultStartingPositions)
        {
            return GetStartingPositionsFromDictionary(startingPlayerPositions, playerAmount);
        }  

        return GetStartingPositionsFromDictionary(GlobalGameStateManager.Instance.startingPlayerPositions, playerAmount);
    }

    List<GridCoordinate> GetStartingPositionsFromDictionary(SerializedDictionary<PlayerAmount, PlayerTransitionInfo> playerTransitionInfos, int playerAmount)
    { 
        return playerTransitionInfos[(PlayerAmount)playerAmount].positions;
    }

    public List<GridCoordinate> GetTransitionGridPositions(Screen screen)
    {
        if (screen.overrideDefaultTransitionGrids)
        {
            return screen.transitionGrids.positions;
        }

        if (overrideDefaultTransitionGrids)
        {
            return transitionGrids.positions;
        }

        return GlobalGameStateManager.Instance.defaultLocationForTransitionGrids.positions;
    }
}
