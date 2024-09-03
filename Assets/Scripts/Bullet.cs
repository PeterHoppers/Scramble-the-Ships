using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : GridMovable
{
    public bool isFriendly = true;
    public ParticleSystem bulletExplosion;
    public AudioClip spawnSound;

    public override void OnPreviewableCreation()
    {
        base.OnPreviewableCreation();
        var audioSource = GetComponent<AudioSource>();

        audioSource.clip = spawnSound;
        audioSource.Play();
    }

    protected override void PerformInteraction(Collider2D collision)
    {
        if (!collision.TryGetComponent<GridObject>(out var collidedPreviewable))
        {
            return;
        }

        if (collidedPreviewable.IsIgnoredByBullets())
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            if (!isFriendly)
            {
                _manager.HandleGridObjectCollision(this, collidedPreviewable);
            }
        }
        else if (collision.CompareTag("Enemy")) //right now, these are the same, but unsure if that'll be true in the future
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
                SetDeathSFX(bulletExplosion);
            }
            _manager.HandleGridObjectCollision(this, collidedPreviewable);
        }
    }
}
