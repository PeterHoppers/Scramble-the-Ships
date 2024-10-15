using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TransformTransition))]
public abstract class Previewable : GridObject
{    
    protected TransformTransition _transitioner;

    [HideInInspector]
    public GameObject previewObject;

    private void Start()
    {
        _transitioner = GetComponent<TransformTransition>();
    }

    public abstract Sprite GetPreviewSprite();

    public virtual Vector2 GetPreviewScale()
    { 
        return transform.localScale * .85f; //slightly smaller so that the original completely covers up the preview
    }

    public virtual Color GetPreviewColor()
    { 
        return new Color(.5f, .5f, .5f, .005f);
    }

    public virtual Color GetPreviewOutline()
    {
        return new Color(4f, 0, 0, .35f);
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

    public IEnumerator WarpToTile(Tile tileDestination, float duration)
    {
        var startingScale = transform.localScale;
        var shrinkScale = new Vector2(.25f, .25f);

        if (previewObject != null)
        { 
            previewObject.transform.localPosition = tileDestination.GetTilePosition();
        }

        var pieceDuration = duration / 3;
        _transitioner.ScaleTo(shrinkScale, pieceDuration);
        yield return new WaitForSeconds(pieceDuration);
        SetPosition(tileDestination);
        yield return new WaitForSeconds(pieceDuration);
        _transitioner.ScaleTo(startingScale, pieceDuration);
    }

    public virtual void UpdateRotationToPreview(float duration)
    {
        if (previewObject == null)
        {
            return;
        }

        var newRotation = previewObject.transform.rotation;
        if (newRotation == GetTransfromAsReference().rotation) 
        {
            return;
        }

        TransitionToRotation(newRotation, duration);
    }

    public virtual void TransitionToRotation(Quaternion newRotation, float duration)
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

    public virtual Transform GetTransfromAsReference()
    {
        return transform;
    }

    protected Vector2 ConvertInputValueToDirection(InputValue input)
    {
        var transformToRef = GetTransfromAsReference();
        switch (input)
        {
            case InputValue.Forward:
            case InputValue.Fire:
                return (Vector2)transformToRef.up;
            case InputValue.Backward:
                return (Vector2)transformToRef.up * -1;
            case InputValue.Port:
                return (Vector2)transformToRef.right * -1;
            case InputValue.Starboard:
                return (Vector2)transformToRef.right;
            default:
                return Vector2.zero;
        }
    }

    protected Quaternion ConvertInputValueToRotation(InputValue input)
    { 
        var currentRotation = GetTransfromAsReference().rotation;
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
