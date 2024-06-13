using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;

public class ScreenManager : MonoBehaviour, IManager
{
    public ScreenChangeTrigger screenTrigger;
    public Screen[] levelScreens;
    GameManager _gameManager;
    int _screenIndex;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnScreenChange += OnScreenChange;
    }

    void OnScreenChange()
    {
        SetupScreen();
    }

    public void SetupScreen()
    {
        var nextScreen = levelScreens[_screenIndex];
        _gameManager.SetScreenStarters(nextScreen.startingItems);
        _gameManager.SetQueuedEnemies(nextScreen.enemySpawnInformation);
        var screenTriggers = _gameManager.SetScreenTranistions(screenTrigger, nextScreen.transitionGrids);
        screenTriggers.ForEach(x => x.OnPlayerEntered += OnPlayerTriggeredScreenChange);
    }

    void OnPlayerTriggeredScreenChange(Player player)
    { 
        _gameManager.PlayerTriggeredScreenChange(player);
    }

    //get message that the screen has finished
    //perform animation for screen transition
    //can either set up the next screen or queue up some cutscenes
}
