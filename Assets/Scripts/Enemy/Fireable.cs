using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireable : GridMovable
{
    [HideInInspector]
    public GridObject owner;
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

        if (collidedPreviewable.CompareTag("Player") || collidedPreviewable.CompareTag("Enemy"))
        {
            if (collidedPreviewable != owner)
            {
                _manager.HandleGridObjectCollision(this, collidedPreviewable);
            }
        }
        else
        {
            //we only do this here because we presume that the player and enemies have their own death effect to play
            if (bulletExplosion)
            {
                if (collidedPreviewable.CannotBeDestoryed())
                {
                    SetDeathSFX(bulletExplosion);
                }
                else
                {
                    collidedPreviewable.SetDeathSFX(bulletExplosion);
                }
            }

            _manager.HandleGridObjectCollision(this, collidedPreviewable);
        }
    }

    public void CollideWith(GridObject collidedPreviewable)
    {
        PerformInteraction(collidedPreviewable); //used by the wormhole to handle weird edgecases
    }
}
