using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PreviewInputHandler : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (GlobalGameStateManager.Instance.GlobalGameStateStatus != GlobalGameStateStatus.Preview) 
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1)) 
        {
            GlobalGameStateManager.Instance.PlayerCount = 1;
            GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            GlobalGameStateManager.Instance.PlayerCount = 2;
            GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
        }
    }
}
