using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenChangeTrigger : GridObject
{
    private SpawnDirections _screenDirection = SpawnDirections.Top;
    private void Awake()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.None;    
    }

    public void SetScreenTransitionDirection(SpawnDirections direction)
    {
        _screenDirection = direction;
    }

    protected override void PerformInteraction(GridObject collidedGridObject)
    {
        if (!collidedGridObject.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        if (collidedGridObject.TryGetComponent<ObstaclePlayer>(out var obstaclePlayer))
        {
            return;
        }

        _manager.ScreenChangeTriggered(playerCollided, _screenDirection);
    }
}
