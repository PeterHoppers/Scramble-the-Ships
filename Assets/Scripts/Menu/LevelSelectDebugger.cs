using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectDebugger : MonoBehaviour
{ 
    // Simple hacky code that allows skipping levels for playesting
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F1))
        {
            PlayLevel(0);
        }
        else if (Input.GetKeyUp(KeyCode.F2))
        {
            PlayLevel(1);
        }
        else if (Input.GetKeyUp(KeyCode.F3))
        {
            PlayLevel(2);
        }
        else if (Input.GetKeyUp(KeyCode.F4))
        {
            PlayLevel(3);
        }
        else if (Input.GetKeyUp(KeyCode.F5))
        {
            PlayLevel(4);
        }
        else if (Input.GetKeyUp(KeyCode.F6))
        {
            PlayLevel(5);
        }
        else if (Input.GetKeyUp(KeyCode.F9))
        {
            PlayLevel(2, 2);
        }
        else if (Input.GetKeyUp(KeyCode.F10))
        {
            PlayLevel(3, 2);
        }
        else if (Input.GetKeyUp(KeyCode.Home))
        {
            GlobalGameStateManager.Instance.ResetGame();
        }
    }

    void PlayLevel(int levelId, int playerCount = 1)
    {
        GlobalGameStateManager.Instance.CreateNewGUID();
        GlobalGameStateManager.Instance.SetPlayerCount(playerCount);
        GlobalGameStateManager.Instance.LoadLevel(levelId);
    }
}
