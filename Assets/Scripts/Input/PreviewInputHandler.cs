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

    PreviewState _currentPreviewState = PreviewState.InsertCoins;
    int _coinsPerPlay;
    int _currentCoints;

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
            UpdateState(PreviewState.FullUnlock);
        }
        else
        {
            _coinsPerPlay = systemSettingParameters.coinsPerPlay;
            CheckIfCanPlay(GlobalGameStateManager.Instance.CreditCount);
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
            if (_currentPreviewState != PreviewState.InsertCoins)
            {
                SelectedOnePlayer();
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            if (_currentPreviewState == PreviewState.FullUnlock)
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

    private void CheckIfCanPlay(int credits)
    {
        if (_coinsPerPlay * 2 <= credits)
        {
            UpdateState(PreviewState.FullUnlock);
        }
        else if (_coinsPerPlay <= credits)
        {
            UpdateState(PreviewState.OnePlayerUnlock);
        }
        else
        {
            UpdateState(PreviewState.InsertCoins);
        }
    }

    void UpdateState(PreviewState newState)
    {
        string displayMessage;
        switch (newState) 
        {
            case PreviewState.InsertCoins:
                displayMessage = $"Credits: {GlobalGameStateManager.Instance.CreditCount}/{_coinsPerPlay}";
                break;
            case PreviewState.OnePlayerUnlock:
                displayMessage = $"Press the 1 Player Button to begin. \n Credits: {GlobalGameStateManager.Instance.CreditCount}/{_coinsPerPlay}";
                break;
            case PreviewState.FullUnlock:
            default:
                displayMessage = "Choose Amount of Players (1 | 2)";
                break;
        }

        previewMessage.text = displayMessage;
        _currentPreviewState = newState;
    }
}

public enum PreviewState
{ 
    InsertCoins,
    OnePlayerUnlock,
    FullUnlock
}
