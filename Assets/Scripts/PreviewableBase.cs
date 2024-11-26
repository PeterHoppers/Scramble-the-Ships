using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OutlineEffect))]
public class PreviewableBase : MonoBehaviour
{
    public List<InputValue> inputsPreviewing { set; get; }

    private SpriteRenderer _spriteRenderer;
    private OutlineEffect _outlineEffect;


    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _outlineEffect = GetComponent<OutlineEffect>();
    }

    public SpriteRenderer GetRenderer()
    { 
        return _spriteRenderer;
    }

    public void SetPreviewOutlineColor(Color color, Sprite previewSprite)
    {
        _outlineEffect.SetSpriteOutline(previewSprite);
        _outlineEffect.SetOutlineColor(color);
        _spriteRenderer.SetPropertyBlock(_outlineEffect.previewMaterialPropertyBlock);
    }

    public void FadeOut(float duration)
    {
        _outlineEffect.FadeOut(_spriteRenderer, duration);
    }
}
