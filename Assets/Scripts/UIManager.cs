using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IManager
{
    public TickDurationUI tickDurationUI;

    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += OnTickStart;
    }

    void OnTickStart(float duration)
    {
        tickDurationUI.TickDuration = duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameManager != null) 
        { 
            tickDurationUI.UpdateTickRemaining(_gameManager.GetTimeRemainingInTick());
        }
    }
}
