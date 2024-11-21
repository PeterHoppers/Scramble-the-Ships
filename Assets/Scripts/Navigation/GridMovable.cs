using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridMovable : Previewable
{
    [HideInInspector]
    public InputValue movingInput = InputValue.None;

    public virtual void SetupMoveable(GameManager manager, SpawnSystem spawnSystem, Tile startingTile)
    {
        base.SetupObject(manager, spawnSystem, startingTile);
        _manager.OnTickStart += CreateNextPreview;
    }

    private void OnDestroy()
    {
        _manager.OnTickStart -= CreateNextPreview; 
        _manager.OnTickStart -= HideMoveable;
        StopAllCoroutines();
    }

    protected virtual void CreateNextPreview(float timeToTickEnd, int currentTickNumber)
    {
        if (movingInput == InputValue.None) 
        {
            return;
        }

        _manager.PreviewMove(this, movingInput);

        if (!_manager.CanPlayerPerformAction(this, movingInput)) 
        {
            RemoveMoveable();
        }
    }

    protected virtual void RemoveMoveable()
    {
        _manager.OnTickStart -= CreateNextPreview;
        _manager.OnTickStart += HideMoveable;
    }

    private void HideMoveable(float timeToTickStart, int currentTickNumber)
    {
        if (this == null) 
        {
            return;        
        }

        StartCoroutine(GoOffScreen(timeToTickStart));
        _manager.OnTickStart -= HideMoveable;
        
        var pauseVFXs = GetComponentsInChildren<VFXPausing>();
        
        foreach (var pause in pauseVFXs) 
        {
            pause.PlayVFX();
            pause.enabled = false;
        }
    }

    private IEnumerator GoOffScreen(float duration)
    {
        var currentPosition = GetCurrentPosition();
        var vectorDirection = ConvertInputValueToDirection(movingInput);
        var targetPosition = _spawnSystem.GetOffscreenPosition(vectorDirection, currentPosition, false);
        var leaveCurve = AnimationCurve.Linear(0, 0, 1, 1);

        TransitionToPosition(targetPosition, duration, leaveCurve);
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
