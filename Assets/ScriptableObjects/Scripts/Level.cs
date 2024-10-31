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

    public int levelNumber;
    public string levelName;
    public AudioClip levelSong;
    public Sprite levelBackground;

    [Header("Level Defaults")]
    public bool overrideDefaultTransitionGrids = false;
    public PlayerTransitionInfo transitionGrids;
    public bool overrideDefaultStartingPositions = false;
    public SerializedDictionary<PlayerAmount, PlayerTransitionInfo> startingPlayerPositions;

    public Screen[] GetLevelScreens(int playerAmount)
    { 
        return (playerAmount == 1 || useOnePlayerForBoth) ? onePlayerLevelScreens : twoPlayerLevelScreens;
    }

    public PlayerTransitionInfo GetStartingPlayerInfo(int playerAmount, Screen screen) 
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

    PlayerTransitionInfo GetStartingPositionsFromDictionary(SerializedDictionary<PlayerAmount, PlayerTransitionInfo> playerTransitionInfos, int playerAmount)
    { 
        return playerTransitionInfos[(PlayerAmount)playerAmount];
    }

    public PlayerTransitionInfo GetTransitionGridInfo(Screen screen)
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

    public string GetDisplayName(bool isMultipleLines = true)
    {
        var divider = (isMultipleLines) ? "<br>" : " ";
        return $"Sector {levelNumber}:{divider}{levelName}";
    }
}
