using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AYellowpaper.SerializedCollections;

public class GameStateDisplay : MonoBehaviour
{
    [SerializeField]
    private SerializedDictionary<GameState, GameObject> gameStateDisplays = new();
    
    // Start is called before the first frame update
    void Awake()
    {
        UpdateStateDisplay(GameState.Waiting);
    }

    public void UpdateStateDisplay(GameState gameState)
    { 
        foreach (var game in gameStateDisplays) 
        {
            game.Value.SetActive((gameState == game.Key));
        }
    }
}
