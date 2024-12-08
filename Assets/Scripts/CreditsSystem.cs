using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class CreditsSystem : MonoBehaviour
{
    public GameObject creditsHolder;
    [Space]
    public TextMeshProUGUI costMessage;
    public TextMeshProUGUI creditMessage;
    public TextMeshProUGUI playerMessage;
    [Space]
    public AudioClip creditAudioClip;

    [Space]
    [SerializeField]
    private InputActionAsset _actions;

    private InputActionMap _uiActionMap;
    private InputAction _coinsInsertedMap;

    public delegate void CoinsChange(int coinsInserted, int creditsEarned);
    public CoinsChange OnCoinsChange;

    int _coinsPerCredit = 1;
    int _coinsInserted = 0;
    bool _isFreeplay = false;
    private void OnEnable()
    {
        GlobalGameStateManager.Instance.OnStateChange += UpdateCreditsBasedOnState;

        _uiActionMap = _actions.FindActionMap("ui");
        _uiActionMap.Enable();
        _coinsInsertedMap = _uiActionMap.FindAction("CoinInserted");

        _coinsInsertedMap.performed += OnCoinInserted;
    }

    private void OnDisable()
    {
        GlobalGameStateManager.Instance.OnStateChange -= UpdateCreditsBasedOnState;

        _coinsInsertedMap.performed -= OnCoinInserted;

        _uiActionMap.Disable();
    }

    private void Start()
    {
        ConfigParameters(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
        OptionsManager.Instance.OnParametersChanged += ConfigParameters;
        UpdateCreditsBasedOnState(GlobalGameStateManager.Instance.GlobalGameStateStatus);

        if (OptionsManager.Instance.systemSettingParameters.isFreeplay)
        {
            OnCoinsChange?.Invoke(0, int.MaxValue);
        }

        UpdateCoinAmount(_coinsInserted);
    }

    private void UpdateCreditsBasedOnState(GlobalGameStateStatus newState)
    {
        creditsHolder.SetActive(newState == GlobalGameStateStatus.Preview || newState == GlobalGameStateStatus.GameOver);
    }

    void ConfigParameters(GameSettingParameters gameSettings, SystemSettingParameters systemSettingParameters)
    {
        if (_isFreeplay && !systemSettingParameters.isFreeplay)
        {
            ClearCoins();
        }

        _isFreeplay = systemSettingParameters.isFreeplay;
        if (_isFreeplay)
        {
            costMessage.text = "Freeplay";
            playerMessage.gameObject.SetActive(false);
            _coinsPerCredit = 1;
            OnCoinsChange?.Invoke(0, int.MaxValue);
        }
        else
        {
            playerMessage.gameObject.SetActive(true);
            costMessage.text = $"{systemSettingParameters.creditDisplay} per Play";
            _coinsPerCredit = systemSettingParameters.coinsPerPlay;            
        }
    }

    private void OnCoinInserted(InputAction.CallbackContext context)
    {
        CoinInserted();
    }

    void CoinInserted()
    {
        _coinsInserted++;
        UpdateCoinAmount(_coinsInserted);
        GlobalAudioManager.Instance.PlayAudioSFX(creditAudioClip);
    }

    public void RemoveCredits(int credits)
    {
        _coinsInserted = _coinsInserted - (credits * _coinsPerCredit);

        if (_coinsInserted < 0) //can techically happen due to freeplay
        {
            _coinsInserted = 0;
        }

        UpdateCoinAmount(_coinsInserted);
    }

    public void ClearCoins()
    {
        _coinsInserted = 0;
        UpdateCoinAmount(_coinsInserted);
    }

    void UpdateCoinAmount(int amount)
    {
        int creditsEarned = amount / _coinsPerCredit;
        int coinsOver = amount % _coinsPerCredit;

        UpdateCreditDisplay(creditsEarned, coinsOver);

        OnCoinsChange?.Invoke(amount, (_isFreeplay) ? int.MaxValue : creditsEarned);
    }

    private void UpdateCreditDisplay(int creditsEarned, int coinsOver)
    {
        var creditText = "";

        if (creditsEarned > 0 || coinsOver == 0)
        {
            creditText += $"{creditsEarned} ";
        }

        if (coinsOver > 0)
        {
            creditText += $"{coinsOver}/{_coinsPerCredit} ";
        }

        creditText += (creditsEarned == 1) ? "Credit" : "Credits";

        creditMessage.text = creditText;
    }
}
