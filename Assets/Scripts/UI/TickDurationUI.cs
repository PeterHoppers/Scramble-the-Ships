using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TickDurationUI : MonoBehaviour
{
    SpriteRenderer _tickDurationSprite;
    public float TickDuration { private get; set; }
    GameManager _manager;

    private MaterialPropertyBlock _tickMaterialPropertyBlock;
    private const int MAX_DEGREES = 360;

    private void Awake()
    {
        _tickDurationSprite = GetComponent<SpriteRenderer>();
        _tickMaterialPropertyBlock = new MaterialPropertyBlock();
        _tickMaterialPropertyBlock.SetFloat("_Angle", 90);
        _tickMaterialPropertyBlock.SetTexture("_MainTex", _tickDurationSprite.sprite.texture);
        _tickDurationSprite.SetPropertyBlock(_tickMaterialPropertyBlock);
    }    

    public void SetupTickListening(GameManager manager)
    {
        _manager = manager;
        _manager.OnTickStart += OnTickStart;
    }

    private void OnDisable()
    {
        if ( _manager != null ) 
        { 
            _manager.OnTickStart -= OnTickStart;
        }
    }

    void Update()
    {
        if (_manager != null)
        {
            UpdateTickRemaining(_manager.GetTimeRemainingInTick());
        }
    }

    void OnTickStart(float duration, int currentTickNumber)
    {
        TickDuration = duration;
    }

    public void SetFilledAmount(float percentageFilled)
    {
        var point = (1 - percentageFilled) * MAX_DEGREES;
        _tickMaterialPropertyBlock.SetFloat("_Arc1", point);
        _tickDurationSprite.SetPropertyBlock(_tickMaterialPropertyBlock);
    }

    public void UpdateTickRemaining(float timeRemainingInTick)
    {
        if (TickDuration == 0)
        {
            SetFilledAmount(0);
            return;
        }

        float precentageRemaining = timeRemainingInTick / TickDuration;
        SetFilledAmount(precentageRemaining);
    }
}
