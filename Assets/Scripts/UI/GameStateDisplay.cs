using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameStateDisplay : MonoBehaviour
{
    TextMeshProUGUI _gameStateText;
    
    // Start is called before the first frame update
    void Awake()
    {
        _gameStateText = GetComponentInChildren<TextMeshProUGUI>();
        UpdateStateDisplay(GameState.Waiting);
    }

    public void UpdateStateDisplay(GameState gameState)
    { 
        switch (gameState) 
        { 
            case GameState.Waiting:
                _gameStateText.text = "Waiting for game to load...";
                break;
            case GameState.GameOver:
                _gameStateText.text = "Game Over";
                break;
            case GameState.Playing:
            default:
                _gameStateText.text = "";
                break;
        }
    }
}
