using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TickDurationUI : MonoBehaviour
{
    Image _tickDurationImage;
    public float TickDuration { private get; set; }
    Vector2 _defaultTickDurationImageSize;

    private void Awake()
    {
        _tickDurationImage = GetComponent<Image>();
        _defaultTickDurationImageSize = _tickDurationImage.rectTransform.sizeDelta;
    }

    public void UpdateTickRemaining(float timeRemainingInTick)
    {
        if (TickDuration == 0)
        {
            _tickDurationImage.rectTransform.sizeDelta = new Vector2(0, 0);
            return;
        }

        float precentageRemaining = timeRemainingInTick / TickDuration;
        _tickDurationImage.rectTransform.sizeDelta = new Vector2(_defaultTickDurationImageSize.x * precentageRemaining, _defaultTickDurationImageSize.y);
    }
}
