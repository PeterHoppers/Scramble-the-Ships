using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour, IManager
{
    public GameObject gameOverHolder;
    public Button retryButton;

    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnGameStateChanged += CheckIfGameOver;
        gameOverHolder.SetActive(false);

        retryButton.onClick.AddListener(() =>
        {
            _gameManager.ContinuePerformed();
        });
    }

    void OnEnable()
    {
        GlobalGameStateManager.Instance.OnCreditsChange += CheckIfCanContinue;
    }

    void OnDisable()
    {
        GlobalGameStateManager.Instance.OnCreditsChange -= CheckIfCanContinue;
    }

    private void CheckIfGameOver(GameState newState)
    {
        if (newState != GameState.GameOver) 
        {
            return;
        }

        gameOverHolder.SetActive(true);
        CheckIfCanContinue(GlobalGameStateManager.Instance.CreditCount);
    }

    private void CheckIfCanContinue(int creditAmount)
    {
        if (creditAmount >= GlobalGameStateManager.Instance.PlayerCount)
        {
            SetPlayAgainState(true);
        }
        else
        {
            SetPlayAgainState(false);
        }
    }

    private void SetPlayAgainState(bool isEnable)
    {
        retryButton.interactable = isEnable;

        if (isEnable)
        {
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }
    }
}
