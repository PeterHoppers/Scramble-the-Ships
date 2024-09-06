using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPickup : GridObject
{
    public PickupType pickupType;
    public AudioClip aquiredSFX;

    protected override void PerformInteraction(Collider2D collision)
    {
        base.PerformInteraction(collision);

        if (!collision.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        GlobalAudioManager.Instance.PlayAudioSFX(aquiredSFX);
        _manager.OnPlayerPickup(playerCollided, pickupType);
        DestroyObject();
    }
}

public enum PickupType
{ 
    Energy
}
