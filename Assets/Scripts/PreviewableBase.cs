using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PreviewableBase : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _previewMaterialPropertyBlock;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _previewMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    public SpriteRenderer GetRenderer()
    { 
        return _spriteRenderer;
    }

    public void SetPreviewOutlineColor(Color color, Sprite previewSprite)
    {
        _previewMaterialPropertyBlock.SetColor("_OutlineColor", color);
        _previewMaterialPropertyBlock.SetTexture("_MainTex", previewSprite.texture);
        _spriteRenderer.SetPropertyBlock(_previewMaterialPropertyBlock);
    }
}
