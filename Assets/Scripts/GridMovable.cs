using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovable : Previewable
{
    public Vector2 travelDirection = Vector2.up;

    public virtual void SetupMoveable(GameManager manager, Tile startingTile)
    {
        base.SetupPreviewable(manager);
        SetTile(startingTile);
        _manager.OnTickStart += CreateNextPreview;
    }

    private void OnDestroy()
    {
        _manager.OnTickStart -= CreateNextPreview;
    }

    private void CreateNextPreview(float timeToTickEnd)
    {
        var previewTile = _manager.AddPreviewAtPosition(this, currentTile, travelDirection);

        if (!previewTile.IsVisible)
        {
            RemoveMoveable();
        }
    }

    protected virtual void RemoveMoveable()
    {
        _manager.OnTickStart -= CreateNextPreview;
    }

    public override Sprite GetPreviewSprite()
    {
        return GetComponentInChildren<SpriteRenderer>().sprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PerformInteraction(collision);
    }

    public virtual void PerformInteraction(Collider2D collision)
    {
       
    }
}
