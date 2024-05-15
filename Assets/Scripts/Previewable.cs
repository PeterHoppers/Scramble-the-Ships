using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Previewable : MonoBehaviour
{
    protected Tile currentTile;
    TransformTransition _transitioner;
    
    public GameObject previewObject;

    private void Start()
    {
        _transitioner = GetComponent<TransformTransition>();
    }
    public abstract Sprite GetPreviewSprite();
    public virtual Vector2 GetCurrentPosition()
    { 
        return transform.position;
    }

    public virtual Color GetPreviewColor()
    { 
        return new Color(.75f, .75f, .75f, .5f);
    }

    public virtual void TransitionToTile(Tile tileDestination, float duration)
    {
        var destination = tileDestination.GetTilePosition();
        _transitioner.MoveTo(destination, duration);
        SetTile(tileDestination);
    }

    public virtual void TransitionToPosition(Vector2 targetPosition, float duration)
    {
        _transitioner.MoveTo(targetPosition, duration);
    }

    public virtual void SetTile(Tile newTile)
    {
        currentTile = newTile;
    }

    //Used for setting a tile directly at a position
    public virtual void SetPosition(Tile directTile)
    {
        if (_transitioner != null)
        {
            _transitioner.StopAllCoroutines();
        }

        currentTile = directTile;
        transform.position = currentTile.GetTilePosition();
    }

    public virtual Vector2 GetGridCoordinates()
    { 
        return currentTile.gridCoordinates;
    }

    public void DestroyPreviewable()
    {
        if (previewObject != null) 
        { 
            Destroy(previewObject);
        }

        Destroy(this.gameObject);
    }
}
