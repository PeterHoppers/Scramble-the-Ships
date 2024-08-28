using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;
using System;

public class PreviewInputHandler : MonoBehaviour
{
    public TextMeshProUGUI previewMessage;

    private void Update()
    {
        if (GlobalGameStateManager.Instance.GlobalGameStateStatus != GlobalGameStateStatus.Preview)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1))
        {
            if (GlobalGameStateManager.Instance.CanPlay(1))
            {
                SelectedOnePlayer();
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            if (GlobalGameStateManager.Instance.CanPlay(2))
            {
                SelectedTwoPlayers();
            }
        }
    }

    private void SelectedOnePlayer()
    {
        GlobalGameStateManager.Instance.SetPlayerCount(1);
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
    }

    private void SelectedTwoPlayers()
    {
        GlobalGameStateManager.Instance.SetPlayerCount(2);
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
    }    
}