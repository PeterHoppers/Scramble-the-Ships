using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHazard : GridObject
{
    protected override void PerformInteraction(Collider2D collision)
    {
        if (!collision.TryGetComponent<GridObject>(out var collidedPreviewable))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            _manager.HandleGridObjectCollision(this, collidedPreviewable);
        }        
    }
}
