using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Previewable : MonoBehaviour
{
    protected Tile currentTile;
    protected GameManager _manager;
    TransformTransition _transitioner;

    [HideInInspector]
    public GameObject previewObject;

    private void Start()
    {
        _transitioner = GetComponent<TransformTransition>();
    }

    public virtual void SetupPreviewable(GameManager manager)
    {
        _manager = manager;
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

    public virtual void DestroyPreviewable()
    {
        if (previewObject != null) 
        { 
            Destroy(previewObject);
        }

        Destroy(this.gameObject);
    }

    protected Vector2 ConvertInputValueToDirection(InputValue input)
    {
        switch (input)
        {
            case InputValue.Up:
            case InputValue.Shoot:
                return (Vector2)transform.up;
            case InputValue.Down:
                return (Vector2)transform.up * -1;
            case InputValue.Left:
                return (Vector2)transform.right * -1;
            case InputValue.Right:
                return (Vector2)transform.right;
            default:
                return Vector2.zero;
        }
    }
}
