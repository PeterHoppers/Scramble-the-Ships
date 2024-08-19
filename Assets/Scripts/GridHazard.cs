using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridHazard : GridObject
{
    public override void SetupObject(GameManager manager, SpawnSystem system, Tile startingTile)
    {
        var childrenObjects = GetComponentsInChildren<GridObject>().Skip(1).ToArray(); //get components in children, for some reason, returns the parent object

        if (childrenObjects.Length > 0)
        {
            foreach (var child in childrenObjects)
            {
                child.SetupObject(manager, system, startingTile);
            }
        }
        else
        { 
            base.SetupObject(manager, system, startingTile);
        }        
    }

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
