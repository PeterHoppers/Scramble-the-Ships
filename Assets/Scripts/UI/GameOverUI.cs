using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverHolder;
    public CountdownUI countdownUI;
    public Button retryButton;
    [Space]
    public AudioClip gameOverJingle;

    void OnEnable()
    {
        GlobalGameStateManager.Instance.OnCreditsChange += CheckIfCanContinue;
    }

    void OnDisable()
    {
        GlobalGameStateManager.Instance.OnCreditsChange -= CheckIfCanContinue;
    }

    public void SetGameOverState(bool isGameOver)
    {
        gameObject.SetActive(isGameOver);
        gameOverHolder.SetActive(isGameOver);

        if (isGameOver)
        {
            GlobalAudioManager.Instance.TransitionSongs(gameOverJingle);
            countdownUI.StartCountdown(() => { GlobalGameStateManager.Instance.ResetGame(); });
            CheckIfCanContinue(GlobalGameStateManager.Instance.CreditCount);
        }
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
        retryButton.gameObject.SetActive(isEnable);

        if (isEnable)
        {
            countdownUI.ResetCountdown();
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }
    }
}
