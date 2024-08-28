using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CreditsDisplayHandler : MonoBehaviour
{
    public GameObject creditsHolder;
    [Space]
    public TextMeshProUGUI costMessage;
    public TextMeshProUGUI creditMessage;

    int _coinsPerPlay = 1;

    private void OnEnable()
    {
        GlobalGameStateManager.Instance.OnStateChange += UpdateCreditsBasedOnState;
    }

    private void OnDisable()
    {
        GlobalGameStateManager.Instance.OnStateChange -= UpdateCreditsBasedOnState;
    }

    private void Start()
    {
        ConfigParameters(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
        OptionsManager.Instance.OnParametersChanged += ConfigParameters;
        UpdateCreditDisplay(GlobalGameStateManager.Instance.CreditCount);
        GlobalGameStateManager.Instance.OnCreditsChange += UpdateCreditDisplay;
        UpdateCreditsBasedOnState(GlobalGameStateManager.Instance.GlobalGameStateStatus);
    }

    private void UpdateCreditsBasedOnState(GlobalGameStateStatus newState)
    {
        print(newState);
        creditsHolder.SetActive(newState == GlobalGameStateStatus.Preview || newState == GlobalGameStateStatus.GameOver);
    }

    void ConfigParameters(GameSettingParameters gameSettings, SystemSettingParameters systemSettingParameters)
    {
        if (systemSettingParameters.isFreeplay)
        {
            costMessage.text = "Freeplay";
            _coinsPerPlay = 1;
        }
        else
        {
            costMessage.text = $"{systemSettingParameters.creditDisplay} per Credit";
            _coinsPerPlay = systemSettingParameters.coinsPerPlay;            
        }
    }

    private void UpdateCreditDisplay(int coinsInserted)
    {
        int creditsEarned = coinsInserted / _coinsPerPlay;
        int creditsOver = coinsInserted % _coinsPerPlay;

        var creditText = "";

        if (creditsEarned > 0 || creditsOver == 0)
        {
            creditText += $"{creditsEarned} ";
        }

        if (creditsOver > 0) 
        {
            creditText += $"{creditsOver}/{_coinsPerPlay} ";
        }

        creditText += (creditsEarned == 1) ? "Credit" : "Credits";

        creditMessage.text = creditText;
    }
}
