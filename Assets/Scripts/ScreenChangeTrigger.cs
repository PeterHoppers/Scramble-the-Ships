using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenChangeTrigger : GridObject
{
    private void Awake()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.None;    
    }

    protected override void PerformInteraction(GridObject collidedGridObject)
    {
        if (!collidedGridObject.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        _manager.ScreenChangeTriggered(playerCollided);
    }
}
