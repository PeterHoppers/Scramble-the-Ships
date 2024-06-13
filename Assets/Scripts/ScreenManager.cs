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
    int _screenIndex = 0;

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
        _screenIndex++;
    }

    void OnPlayerTriggeredScreenChange(Player player)
    {
        StartCoroutine(PerformAnimation(player));        
    }

    IEnumerator PerformAnimation(Player player)
    {
        animator.Play("pan");
        _gameManager.PlayerTriggeredScreenChange(player);
        yield return new WaitForSeconds(1f); //todo: hook this up so that it actually knows how long the animatation should go for
        _gameManager.ClearObjects(); 
        yield return new WaitForSeconds(.25f);
        animator.Play("close");
        yield return new WaitForSeconds(1f);
        _gameManager.ScreenAnimationChangeFinished();
    }

    //get message that the screen has finished
    //perform animation for screen transition
    //can either set up the next screen or queue up some cutscenes
}
