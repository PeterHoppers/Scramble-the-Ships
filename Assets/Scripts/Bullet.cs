using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : GridMovable
{
    public bool isFriendly = true;

    public override void PerformInteraction(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isFriendly)
        {
            collision.TryGetComponent<Player>(out var collidedPlayer);
            collidedPlayer.OnHit(this);
        }
        else if (collision.CompareTag("Enemy") && isFriendly)
        {
            print("Destoryed Enemy!");
        }
    }
}
