using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovable : Previewable
{
    [HideInInspector]
    public Vector2 travelDirection = Vector2.up;

    private SpawnSystem _spawnSystem;

    public virtual void SetupMoveable(GameManager manager, SpawnSystem spawnSystem, Tile startingTile)
    {
        base.SetupObject(manager);
        CurrentTile = startingTile;
        _manager.OnTickStart += CreateNextPreview;
        _spawnSystem = spawnSystem;
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
        gameObject.SetActive(false);
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
