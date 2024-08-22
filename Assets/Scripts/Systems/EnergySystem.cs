using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    public delegate void EnergyChange(int currentEnergy);
    public EnergyChange OnEnergyChange;

    public int energyPerMove;
    public int energyPerFire;
    public int energyPerLifeLoss;

    public int energyRegainedOnScreenEnd;

    public int maxEnergy;
    int _currentEnergy;
    int _playerCount = 1;

    GameManager _gameManager;

    public int CurrentEnergy
    { 
        get { return _currentEnergy; }
        set 
        {
            if (value > maxEnergy * _playerCount)
            { 
                value = maxEnergy * _playerCount;
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
    }

    public void SetEnergy(int playerCount)
    {
        _playerCount = playerCount;
        CurrentEnergy = maxEnergy * _playerCount;
    }

    private void OnTickEnd(int _)
    {
        CurrentEnergy -= energyPerMove * _gameManager.GetPlayersRemaining();
    }

    private void OnScreenChange(int screensRemaining)
    {
        CurrentEnergy += energyRegainedOnScreenEnd;
    }

    public int OnPlayerFired()
    {
        CurrentEnergy -= (energyPerFire - energyPerMove);
        return CurrentEnergy;
    }

    public int OnPlayerDied()
    { 
        CurrentEnergy -= energyPerLifeLoss;
        return CurrentEnergy;
    }
}
