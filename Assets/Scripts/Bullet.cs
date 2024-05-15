using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : GridMovable
{
    public bool isFriendly = true;

    protected override void RemoveMoveable()
    {
        base.RemoveMoveable();
        _manager.OnTickStart += HideBullet;
    }

    private void HideBullet(float timeToTickStart)
    {
        StartCoroutine(GoOffScreen(timeToTickStart));
        _manager.OnTickStart -= HideBullet;
    }

    private IEnumerator GoOffScreen(float duration)
    {
        var currentPosition = GetCurrentPosition();
        //TODO: Pull number out into own 'off screen' position
        var targetPosition = currentPosition + travelDirection * 3;
        TransitionToPosition(targetPosition, duration);
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }

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
