using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OutlineEffect : MonoBehaviour
{
    public MaterialPropertyBlock previewMaterialPropertyBlock { get; private set; }
    [SerializeField]
    private AnimationCurve _previewFadeOutCurve;

    const string TEXTURE_PROP = "_MainTex";
    const string COLOR_PROP = "_OutlineColor";
    const string OUTLINE_SIZE_PROP = "_OutlineSize";
    const float TARGET_FADE_OUT_OPACITY = 0f;

    private void Awake()
    {
        previewMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    public void SetSpriteOutline(Sprite previewSprite)
    {
        previewMaterialPropertyBlock.SetTexture(TEXTURE_PROP, previewSprite.texture);
    }

    public void SetOutlineColor(Color color)
    {
        previewMaterialPropertyBlock.SetColor(COLOR_PROP, color);
    }

    public void SetOutlineWidth(int lineWidth) 
    {
        previewMaterialPropertyBlock.SetInt(OUTLINE_SIZE_PROP, lineWidth);
    }

    public Color GetOutlineColor()
    {
        return previewMaterialPropertyBlock.GetColor(COLOR_PROP);
    }

    public void FadeOut(SpriteRenderer renderer, float duration)
    {
        StartCoroutine(AnimateFadeOut(renderer, TARGET_FADE_OUT_OPACITY, duration));
    }

    IEnumerator AnimateFadeOut(SpriteRenderer renderer, float targetOpacity, float duration)
    {
        float journey = 0f;
        var outlineColor = GetOutlineColor();
        var startingAlpha = outlineColor.a;

        while (journey <= duration)
        {
            journey += Time.deltaTime;
            float curvePercent = GetCurvePercent(journey, duration, _previewFadeOutCurve);
            float lerpedAlpa = Mathf.LerpUnclamped(startingAlpha, targetOpacity, curvePercent);

            var newColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, lerpedAlpa);
            SetOutlineColor(newColor);
            renderer.SetPropertyBlock(previewMaterialPropertyBlock);

            yield return null;
        }
    }

    float GetCurvePercent(float journey, float duration, AnimationCurve curve)
    {
        float percent = Mathf.Clamp01(journey / duration);
        return curve.Evaluate(percent);
    }
}
