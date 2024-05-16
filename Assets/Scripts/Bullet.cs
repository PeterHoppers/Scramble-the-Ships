using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : GridMovable
{
    public bool isFriendly = true;

    protected override void PerformInteraction(Collider2D collision)
    {
        if (!collision.TryGetComponent<Previewable>(out var collidedPreviewable))
        {
            return;
        }

        if (collision.CompareTag("Player") && !isFriendly)
        {            
            _manager.PreviewablesCollided(this, collidedPreviewable);
        }
        else if (collision.CompareTag("Enemy") && isFriendly) //right now, these are the same, but unsure if that'll be true in the future
        {
            _manager.PreviewablesCollided(this, collidedPreviewable);
        }
    }
}
