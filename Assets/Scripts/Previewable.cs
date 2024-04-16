using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Previewable : MonoBehaviour
{
    TransformTransition _transitioner;

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

    public virtual void Move(Vector2 destination, float duration)
    {
        _transitioner.MoveTo(destination, duration);
    }
}
