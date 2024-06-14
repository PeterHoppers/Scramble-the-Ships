using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovable : Previewable
{
    [HideInInspector]
    public Vector2 travelDirection = Vector2.up;

    public virtual void SetupMoveable(GameManager manager, SpawnSystem spawnSystem, Tile startingTile)
    {
        base.SetupObject(manager, spawnSystem);
        CurrentTile = startingTile;
        _manager.OnTickStart += CreateNextPreview;
    }

    private void OnDestroy()
    {
        _manager.OnTickStart -= CreateNextPreview;
        StopAllCoroutines();
    }

    protected virtual void CreateNextPreview(float timeToTickEnd)
    {
        if (travelDirection == Vector2.zero) 
        {
            return;
        }

        var previewTile = _manager.AddPreviewAtPosition(this, CurrentTile, travelDirection);

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
        var targetPosition = _spawnSystem.GetOffscreenPosition(travelDirection, currentPosition, false);
        TransitionToPosition(targetPosition, duration);
        yield return new WaitForSeconds(duration);
        _spawnSystem.DespawnObject(this);
    }

    public override Sprite GetPreviewSprite()
    {
        return GetComponentInChildren<SpriteRenderer>().sprite;
    }

    protected virtual Vector2 GetCurrentPosition()
    {
        return transform.localPosition;
    }
}
