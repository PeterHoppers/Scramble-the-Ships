using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IManager
{
    public Image tickDurationImage;

    float _tickDuration = 0;
    Vector2 _defaultTickDurationImageSize;
    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += OnTickStart;
        _defaultTickDurationImageSize = tickDurationImage.rectTransform.sizeDelta;
    }

    void OnTickStart(float duration)
    { 
        _tickDuration = duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameManager != null) 
        { 
            float precentageRemaining = _gameManager.GetTimeRemainingInTick() / _tickDuration;
            tickDurationImage.rectTransform.sizeDelta = new Vector2(_defaultTickDurationImageSize.x * precentageRemaining, _defaultTickDurationImageSize.y);
        }
    }
}
