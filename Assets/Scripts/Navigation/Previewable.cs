using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Previewable : GridObject
{    
    TransformTransition _transitioner;

    [HideInInspector]
    public GameObject previewObject;

    private void Start()
    {
        _transitioner = GetComponent<TransformTransition>();
    }

    public abstract Sprite GetPreviewSprite();

    public virtual Color GetPreviewColor()
    { 
        return new Color(.5f, .5f, .5f, .25f);
    }

    public virtual void TransitionToTile(Tile tileDestination, float duration)
    {
        var destination = tileDestination.GetTilePosition();
        if (_transitioner == null)
        {
            _transitioner = GetComponent<TransformTransition>();
        }

        _transitioner.MoveTo(destination, duration);
        CurrentTile = tileDestination;
    }

    public virtual void TransitionToPosition(Vector2 targetPosition, float duration)
    {
        if (_transitioner == null)
        {
            _transitioner = GetComponent<TransformTransition>();
        }

        _transitioner.MoveTo(targetPosition, duration);
    }

    public override void SetPosition(Tile directTile)
    {
        if (_transitioner != null)
        {
            _transitioner.StopAllCoroutines();
        }

        base.SetPosition(directTile);
    }

    public override void DestroyObject()
    {
        if (previewObject != null) 
        { 
            Destroy(previewObject);
        }

        base.DestroyObject();
    }

    protected Vector2 ConvertInputValueToDirection(InputValue input)
    {
        switch (input)
        {
            case InputValue.Forward:
            case InputValue.Fire:
                return (Vector2)transform.up;
            case InputValue.Backward:
                return (Vector2)transform.up * -1;
            case InputValue.Port:
                return (Vector2)transform.right * -1;
            case InputValue.Starboard:
                return (Vector2)transform.right;
            default:
                return Vector2.zero;
        }
    }
}