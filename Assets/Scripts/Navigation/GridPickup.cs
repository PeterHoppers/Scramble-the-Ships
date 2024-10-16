using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPickup : GridObject
{
    public PickupType pickupType;
    public AudioClip aquiredSFX;

    protected override void PerformInteraction(GridObject collidedGridObject)
    {
        base.PerformInteraction(collidedGridObject);

        if (!collidedGridObject.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        GlobalAudioManager.Instance.PlayAudioSFX(aquiredSFX);
        _manager.OnPlayerPickup(playerCollided, pickupType);
        RemovePickup();
    }

    public void RemovePickup()
    {
        DestroyObject();
    }
}

public enum PickupType
{ 
    Energy
}
