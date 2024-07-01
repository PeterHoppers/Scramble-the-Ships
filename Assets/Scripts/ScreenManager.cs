using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;

public class ScreenManager : MonoBehaviour, IManager
{
    public Animator animator;
    public ScreenChangeTrigger screenTrigger;
    public Screen[] levelScreens;
    GameManager _gameManager;
    int _screenAmount = 0;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnScreenChange += OnScreenChange;
        _gameManager.OnLevelEnd += OnLevelEnd;
        _screenAmount = levelScreens.Length;
        _gameManager.SetLevelInformation(_screenAmount);
    }

    void OnLevelEnd(int _)
    {
        StopAllCoroutines();
    }

    void OnScreenChange(int screensRemaining, float tickDuration)
    {
        int levelIndex = _screenAmount - screensRemaining;
        var nextScreen = levelScreens[levelIndex];

        if (tickDuration > 0)
        {
            StartCoroutine(SceenChange(tickDuration, nextScreen));
        }
        else
        {
            _gameManager.SetupNextScreen(nextScreen, screenTrigger);
            StartCoroutine(_gameManager.ScreenLoaded());
        }
    }

    //Screen Change Event
    //hides the screen with duration based upon time given in screen change event
    //gives the game manager the information needed to set up the next screen
    //reveals the new screen based upon a time given in the screen change event
    IEnumerator SceenChange(float tickDuration, Screen nextScreen)
    {
        animator.SetFloat("animSpeed", tickDuration); //since our animations are set to being 1.0s, this will change our animation to be whatever the tick duration is
        animator.Play("pan");
        yield return new WaitForSeconds(tickDuration);
        _gameManager.SetupNextScreen(nextScreen, screenTrigger);
        animator.Play("close");
        yield return new WaitForSeconds(tickDuration);
        StartCoroutine(_gameManager.ScreenLoaded());
    }
}
