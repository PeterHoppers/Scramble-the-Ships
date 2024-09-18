using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridHazard : GridObject
{
    protected override void PerformInteraction(GridObject collidedPreviewable)
    {
        if (collidedPreviewable.CompareTag("Player"))
        {
            _manager.HandleGridObjectCollision(this, collidedPreviewable);
        }        
    }
}
