using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    public delegate void EnergyChange(int currentEnergy);
    public EnergyChange OnEnergyChange;

    public int energyRegainedOnScreenEnd;
    public int minEnergyOnScreenStart;

    int _energyPerMove;
    int _energyPerFire;
    int _energyPerLifeLoss;
    int _maxEnergy;
    int _maxEnergyPerPlayer;
    int _currentEnergy;
    int _playerCount = 1;

    int _energyAtScreenStart;

    GameManager _gameManager;

    public int CurrentEnergy
    { 
        get { return _currentEnergy; }
        set 
        {
            if (value > _maxEnergy)
            { 
                value = _maxEnergy;
            }

            _currentEnergy = value;
            OnEnergyChange?.Invoke(value);        
        }
    }

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _gameManager.OnTickEnd += OnTickEnd;
        _gameManager.OnScreenChange += OnScreenChange;
        _gameManager.OnScreenResetStart += OnScreenResetStart;
        _gameManager.OnScreenResetEnd += OnScreenResetEnd;
        _gameManager.EffectsSystem.OnMaxEnergyChanged += OnMaxEnergyPerPersonChanged;

        if (OptionsManager.Instance != null) 
        {
            OptionsManager.Instance.OnParametersChanged += OnParametersChanged;
            OnParametersChanged(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
        }        
    }

    private void OnParametersChanged(GameSettingParameters gameSettings, SystemSettingParameters _)
    {
        _energyPerMove = gameSettings.energyPerMove;
        _energyPerFire = gameSettings.energyPerShot;
        _energyPerLifeLoss = gameSettings.energyPerDeath;

        OnMaxEnergyPerPersonChanged(gameSettings.maxEnergy);
    }

    public void SetEnergy(int playerCount)
    {
        UpdateMaxEnergy(_maxEnergyPerPlayer, playerCount);
    }

    void OnMaxEnergyPerPersonChanged(int newMaxEnergyPerPerson)
    {
        UpdateMaxEnergy(newMaxEnergyPerPerson, _playerCount);
    }

    void UpdateMaxEnergy(int maxEnergyPerPerson, int playerCount)
    {
        _maxEnergyPerPlayer = maxEnergyPerPerson;
        _playerCount = playerCount;
        _maxEnergy = _maxEnergyPerPlayer * _playerCount;
        CurrentEnergy = _maxEnergy;
        _energyAtScreenStart = CurrentEnergy;
    }

    private void OnTickEnd(int _)
    {
        CurrentEnergy -= _energyPerMove * _gameManager.GetPlayersRemaining();
    }

    private void OnScreenChange(int screensRemaining)
    {
        var energyToHave = CurrentEnergy + (energyRegainedOnScreenEnd * _playerCount);
        
        if (energyToHave < (_maxEnergy / 2))
        {
            energyToHave += energyRegainedOnScreenEnd / 2 * _playerCount;
        } 

        if (energyToHave < minEnergyOnScreenStart * _playerCount)
        {
            energyToHave = minEnergyOnScreenStart * _playerCount;
        }

        CurrentEnergy = energyToHave;
        _energyAtScreenStart = CurrentEnergy;
    }

    void OnScreenResetStart()
    {
        CurrentEnergy = _energyAtScreenStart;
    }

    void OnScreenResetEnd()
    {
        CurrentEnergy -= _energyPerLifeLoss;
        _energyAtScreenStart = CurrentEnergy;
    }

    public int OnPlayerFired()
    {
        CurrentEnergy -= (_energyPerFire - _energyPerMove);
        return CurrentEnergy;
    }

    //use this to update the UI after the reset happens to highlight the change in value
    public bool CanPlayerDieAndGameContinue()
    { 
        return (CurrentEnergy - _energyPerLifeLoss > 0);
    }
}
