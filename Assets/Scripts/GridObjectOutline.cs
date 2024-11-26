using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OutlineEffect))]
public class GridObjectOutline : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _rendererToOutline;
    private SpriteRenderer _outlineRenderer;
    private OutlineEffect _outlineEffect;

    [SerializeField]
    [ColorUsage(true, true)]
    private Color _outlineColor;
    [SerializeField]
    private int _outlineSize;

    private void Awake()
    {
        _outlineRenderer = GetComponent<SpriteRenderer>();
        _outlineEffect = GetComponent<OutlineEffect>();
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(DoesRendererHaveSprite);
        _outlineRenderer.sprite = _rendererToOutline.sprite;
        _outlineRenderer.sortingLayerID = _rendererToOutline.sortingLayerID;
        _outlineRenderer.sortingOrder = _rendererToOutline.sortingOrder;
        _outlineEffect.SetOutlineColor(_outlineColor);
        _outlineEffect.SetOutlineWidth(_outlineSize);
        _outlineEffect.SetSpriteOutline(_rendererToOutline.sprite);

        _outlineRenderer.SetPropertyBlock(_outlineEffect.previewMaterialPropertyBlock);
    }

    bool DoesRendererHaveSprite()
    {
        return _rendererToOutline.sprite != null;
    }
}
