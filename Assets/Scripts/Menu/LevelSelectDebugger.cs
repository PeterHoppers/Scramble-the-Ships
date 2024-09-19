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
            PlayLevel(1);
        }
        else if (Input.GetKeyUp(KeyCode.F2))
        {
            PlayLevel(2);
        }
        else if (Input.GetKeyUp(KeyCode.F3))
        {
            PlayLevel(3);
        }
        else if (Input.GetKeyUp(KeyCode.F4))
        {
            PlayLevel(4);
        }
    }

    void PlayLevel(int levelId)
    { 
        GlobalGameStateManager.Instance.SetLevel(levelId);
    }
}
