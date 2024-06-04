using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;

public class ScreenManager : MonoBehaviour, IManager
{
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
        _gameManager.UpdateScreenInformation(levelScreens[_screenIndex]);
    }
}
