using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndestoryableMovable : GridMovable
{
    private void Awake()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.Undestroyable;
    }

    protected override void PerformInteraction(GridObject collidedPreviewable)
    {
        if (collidedPreviewable.CompareTag("Player"))
        {
            _manager.HandleGridObjectCollision(this, collidedPreviewable);
        }
    }
}
