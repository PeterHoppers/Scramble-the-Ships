using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : MonoBehaviour
{
    [SerializeField]
    private float _countdownDurationInSeconds;

    private TextMeshProUGUI _countdownDisplay;
    private AudioSource _countdownAudioSource;
    private Image _countdownImage;
    private float _secondsRemaining;
    private Action _actionToPerfrom;

    private void Awake()
    {
        _countdownDisplay = GetComponentInChildren<TextMeshProUGUI>();
        _countdownImage = GetComponentInChildren<Image>();
        _countdownAudioSource = GetComponent<AudioSource>();
    }

    public void StartCountdown(Action finishCountdownAction)
    {
        _actionToPerfrom = finishCountdownAction;
        ResetCountdown();
    }

    public void ResetCountdown()
    { 
        StopAllCoroutines();
        _secondsRemaining = _countdownDurationInSeconds;
        StartCoroutine(PerformCountdown());
    }

    IEnumerator PerformCountdown()
    {
        UpdateVisuals();
        yield return new WaitForSeconds(1f);
        _secondsRemaining--;

        if (_secondsRemaining >= 0)
        {
            StartCoroutine(PerformCountdown());
        }
        else
        {
            _actionToPerfrom?.Invoke();
        }
    }

    void UpdateVisuals()
    {
        var countdownPercent = _secondsRemaining / _countdownDurationInSeconds;

        if (countdownPercent < 1)
        {
            _countdownAudioSource.Play();
        }

        _countdownImage.fillAmount = countdownPercent;
        _countdownDisplay.text = _secondsRemaining.ToString();
    }
}
