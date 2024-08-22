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

    GameManager _gameManager;

    public int CurrentEnergy
    { 
        get { return _currentEnergy; }
        set 
        {
            if (value > maxEnergy)
            { 
                value = maxEnergy;
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

        CurrentEnergy = maxEnergy;
    }

    private void OnTickEnd(int _)
    {
        CurrentEnergy -= energyPerMove;
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
