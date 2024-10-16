using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    public delegate void EnergyChange(int currentEnergy, int maxEnergy);
    public EnergyChange OnEnergyChange;

    public int energyRegainedOnScreenEnd;
    public int minEnergyOnScreenStart;
    public int energyRegainedOnPickup;

    int _energyPerMove;
    int _energyPerFire;
    int _energyPerLifeLoss;
    bool _isEnergyRestoredOnDeath;
    int _maxEnergy;
    int _maxEnergyPerPlayer;
    int _currentEnergy;
    int _playerCount = 1;

    int _energyAtScreenStart;

    GameManager _gameManager;
    bool _hasCollectedEnergyPickupForThisScreen = false; //todo: if we ever make a screen with multiple energy pick-ups, we'll need to change this

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
            OnEnergyChange?.Invoke(value, _maxEnergy);        
        }
    }

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _gameManager.OnTickEnd += OnTickEnd;
        _gameManager.OnScreenChange += OnScreenChange;
        _gameManager.OnScreenResetStart += OnScreenResetStart;
        _gameManager.OnScreenResetEnd += OnScreenResetEnd;
        _gameManager.OnPlayerPickup += OnPlayerPickup;

        _gameManager.EffectsSystem.OnMaxEnergyChanged += OnMaxEnergyPerPersonChanged;
    }

    private void OnEnable()
    {
        if (OptionsManager.Instance != null)
        {
            OptionsManager.Instance.OnParametersChanged += OnParametersChanged;
            OnParametersChanged(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
        }
    }

    private void OnDisable()
    {
        if (OptionsManager.Instance != null)
        {
            OptionsManager.Instance.OnParametersChanged -= OnParametersChanged;
        }

        if (_gameManager != null) 
        {
            _gameManager.EffectsSystem.OnMaxEnergyChanged -= OnMaxEnergyPerPersonChanged;
        }
    }

    private void OnParametersChanged(GameSettingParameters gameSettings, SystemSettingParameters _)
    {
        _energyPerMove = gameSettings.energyPerMove;
        _energyPerFire = gameSettings.energyPerShot;
        _energyPerLifeLoss = gameSettings.energyPerDeath;
        _isEnergyRestoredOnDeath = gameSettings.isEnergyRestoredToStartOnDeath;

        OnMaxEnergyPerPersonChanged(gameSettings.maxEnergy);
    }

    public void SetEnergy(int playerCount)
    {
        UpdateMaxEnergy(_maxEnergyPerPlayer, playerCount);
    }

    public void RefillEnergy()
    {
        CurrentEnergy = _maxEnergy;
        _energyAtScreenStart = _maxEnergy;
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

    private void OnTickEnd(float tickEndDuration, int _)
    {
        CurrentEnergy -= _energyPerMove * _gameManager.GetPlayersRemaining();
    }

    private void OnScreenChange(int _, int max_)
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
        _hasCollectedEnergyPickupForThisScreen = false;
    }

    void OnScreenResetStart()
    {
        if (_isEnergyRestoredOnDeath)
        {
            CurrentEnergy = _energyAtScreenStart;
        }
    }

    void OnScreenResetEnd()
    {
        CurrentEnergy -= GetEnergyLossWhenDied();
        _energyAtScreenStart = CurrentEnergy;

        if (_hasCollectedEnergyPickupForThisScreen)
        {
            var energyPickup = GameObject.FindGameObjectsWithTag("Pickup");
            foreach(var pickupGO in energyPickup) 
            {
                if (pickupGO.TryGetComponent<GridPickup>(out var gridPickup))
                {
                    if (gridPickup.pickupType == PickupType.Energy)
                    {
                        gridPickup.RemovePickup();
                    }
                }
            }
        }
    }

    private void OnPlayerPickup(Player _, PickupType pickupType)
    {
        if (pickupType == PickupType.Energy)
        {
            CurrentEnergy += energyRegainedOnPickup * _playerCount;
            _hasCollectedEnergyPickupForThisScreen = true;
        }
    }

    public int OnPlayerFired()
    {
        CurrentEnergy -= (_energyPerFire - _energyPerMove);
        return CurrentEnergy;
    }

    //use this to update the UI after the reset happens to highlight the change in value
    public bool CanPlayerDieAndGameContinue()
    { 
        return (CurrentEnergy - GetEnergyLossWhenDied() > 0);
    }

    int GetEnergyLossWhenDied()
    {
        return _energyPerLifeLoss * _playerCount;
    }
}
