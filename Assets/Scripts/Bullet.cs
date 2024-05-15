using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Previewable
{
    public bool isFriendly = true;
    public Vector2 travelDirection = Vector2.up;
    GameManager _manager;
    Collider2D _bulletCollider;

    void Awake()
    {
        _bulletCollider = GetComponent<Collider2D>();
    }

    public void SetupBullet(GameManager manager, Tile startingTile)
    {
        _manager = manager;
        SetTile(startingTile);
        _manager.OnTickStart += CreateNextPreview;
    }

    private void OnDestroy()
    {
        _manager.OnTickStart -= CreateNextPreview;
    }

    private void CreateNextPreview(float timeToTickEnd)
    {
        var previewTile = _manager.AddPreviewAtPosition(this, currentTile, travelDirection);

        if (!previewTile.IsVisible) 
        {
            _manager.OnTickStart -= CreateNextPreview;
            _manager.OnTickStart += HideBullet;
        }
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

    public override Sprite GetPreviewSprite()
    {
        return GetComponentInChildren<SpriteRenderer>().sprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
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
