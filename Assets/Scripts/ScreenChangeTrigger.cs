using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenChangeTrigger : GridObject
{
    public delegate void PlayerEntered(Player playerEntered);
    public PlayerEntered OnPlayerEntered;

    protected override void PerformInteraction(Collider2D collision)
    {
        if (!collision.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        OnPlayerEntered?.Invoke(playerCollided);
    }
}
