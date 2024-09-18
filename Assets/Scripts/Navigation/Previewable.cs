using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TransformTransition))]
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

    public virtual Vector2 GetPreviewScale()
    { 
        return transform.localScale * .95f; //slightly smaller so that the original completely covers up the preview
    }

    public virtual Color GetPreviewColor()
    { 
        return new Color(.5f, .5f, .5f, .005f);
    }

    public virtual Color GetPreviewOutline()
    {
        return new Color(1f, 0, 0, .35f);
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

    public virtual void UpdateRotationToPreview(float duration)
    {
        if (previewObject == null)
        {
            return;
        }

        var newRotation = previewObject.transform.rotation;
        if (newRotation == transform.rotation) 
        {
            return;
        }

        TransitionToRotation(newRotation, duration);
    }

    protected virtual void TransitionToRotation(Quaternion newRotation, float duration)
    {
        if (_transitioner == null)
        {
            _transitioner = GetComponent<TransformTransition>();
        }

        _transitioner.RotateTo(newRotation, duration);
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

    public virtual void CreatedNewPreviewable(Previewable createdPreviewabled)
    { }

    public virtual void OnPreviewableCreation()
    {
        gameObject.SetActive(true);
    }

    public virtual void ResolvePreviewable()
    { }

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

    protected Quaternion ConvertInputValueToRotation(InputValue input)
    { 
        var currentRotation = transform.rotation;
        switch (input) 
        {
            case InputValue.Clockwise:
                return currentRotation *= Quaternion.Euler(0, 0, -90f);
            case InputValue.Counterclockwise:
                return currentRotation *= Quaternion.Euler(0, 0, 90f);
            default:
                return currentRotation;
        }
    }
}
