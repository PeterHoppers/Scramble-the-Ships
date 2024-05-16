using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovable : Previewable
{
    [HideInInspector]
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

    protected virtual void CreateNextPreview(float timeToTickEnd)
    {
        if (travelDirection == Vector2.zero) 
        {
            return;
        }

        var previewTile = _manager.AddPreviewAtPosition(this, currentTile, travelDirection);

        if (!previewTile.IsVisible)
        {
            RemoveMoveable();
        }
    }

    protected virtual void RemoveMoveable()
    {
        _manager.OnTickStart -= CreateNextPreview;
        _manager.OnTickStart += HideMoveable;
    }

    private void HideMoveable(float timeToTickStart)
    {
        StartCoroutine(GoOffScreen(timeToTickStart));
        _manager.OnTickStart -= HideMoveable;
    }

    private IEnumerator GoOffScreen(float duration)
    {
        var currentPosition = GetCurrentPosition();
        //TODO: Pull number out into own 'off screen' position
        var targetPosition = currentPosition + travelDirection * 3;
        TransitionToPosition(targetPosition, duration);
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
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
