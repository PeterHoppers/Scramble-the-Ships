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

    int _coinsPerPlay;
    double _currentCredits = 0;

    private void Start()
    {
        ConfigParameters(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
        OptionsManager.Instance.OnParametersChanged += ConfigParameters;
        GlobalGameStateManager.Instance.OnCreditsChange += CheckIfCanPlay;
    }

    void ConfigParameters(GameSettingParameters gameSettings, SystemSettingParameters systemSettingParameters)
    {
        if (systemSettingParameters.isFreeplay)
        {
            _coinsPerPlay = 0;
        }
        else
        {
            _coinsPerPlay = systemSettingParameters.coinsPerPlay;
        }
    }

    private void CheckIfCanPlay(int insertedCoins)
    {
        if (_coinsPerPlay == 0)
        {
            _currentCredits = int.MaxValue;
        }
        else
        {
            _currentCredits = insertedCoins / _coinsPerPlay;
        }
    }

    private void Update()
    {
        if (GlobalGameStateManager.Instance.GlobalGameStateStatus != GlobalGameStateStatus.Preview)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1))
        {
            if (_currentCredits >= 1)
            {
                SelectedOnePlayer();
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            if (_currentCredits >= 2)
            {
                SelectedTwoPlayers();
            }
        }
    }

    private void SelectedOnePlayer()
    {
        GlobalGameStateManager.Instance.PlayerCount = 1;
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
    }

    private void SelectedTwoPlayers()
    {
        GlobalGameStateManager.Instance.PlayerCount = 2;
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
    }    
}