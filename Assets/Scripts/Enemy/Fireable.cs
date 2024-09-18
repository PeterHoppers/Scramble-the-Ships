using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireable : GridMovable
{
    public bool isFriendly = true;
    public ParticleSystem bulletExplosion;

    public virtual void OnOwnerInputChange(GridMovable owner, GameManager gameManager, InputValue previewInput)
    { 
        
    }

    protected override void PerformInteraction(GridObject collidedPreviewable)
    {
        if (collidedPreviewable.IsIgnoredByBullets())
        {
            return;
        }

        if (collidedPreviewable.CompareTag("Player"))
        {
            if (!isFriendly)
            {
                _manager.HandleGridObjectCollision(this, collidedPreviewable);
            }
        }
        else if (collidedPreviewable.CompareTag("Enemy")) //right now, these are the same, but unsure if that'll be true in the future
        {
            if (isFriendly)
            {
                _manager.HandleGridObjectCollision(this, collidedPreviewable);
            }
        }
        else
        {
            //we only do this here because we presume that the player and enemies have their own death effect to play
            if (bulletExplosion)
            {
                collidedPreviewable.SetDeathSFX(bulletExplosion);
            }

            _manager.HandleGridObjectCollision(this, collidedPreviewable);
        }
    }
}
