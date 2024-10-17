using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PreviewableBase : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _previewMaterialPropertyBlock;
    [SerializeField]
    private AnimationCurve _previewFadeOutCurve;

    private const float TARGET_FADE_OUT_OPACITY = 0f;

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

    public void FadeOut(float duration)
    {        
        StartCoroutine(AnimateFadeOut(TARGET_FADE_OUT_OPACITY, duration));
    }

    IEnumerator AnimateFadeOut(float targetOpacity, float duration)
    {
        float journey = 0f;
        var outlineColor = _previewMaterialPropertyBlock.GetColor("_OutlineColor");
        var startingAlpha = outlineColor.a;
        
        while (journey <= duration)
        {
            journey += Time.deltaTime;
            float curvePercent = GetCurvePercent(journey, duration, _previewFadeOutCurve);
            float lerpedAlpa = Mathf.LerpUnclamped(startingAlpha, targetOpacity, curvePercent);

            var newColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, lerpedAlpa);
            _previewMaterialPropertyBlock.SetColor("_OutlineColor", newColor);
            _spriteRenderer.SetPropertyBlock(_previewMaterialPropertyBlock);

            yield return null;
        }
    }

    float GetCurvePercent(float journey, float duration, AnimationCurve curve)
    {
        float percent = Mathf.Clamp01(journey / duration);
        return curve.Evaluate(percent);
    }
}
