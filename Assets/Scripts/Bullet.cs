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
        var isOnGrid = _manager.AddPreviewAtPosition(this, travelDirection, GetGridCoordinates());

        if (!isOnGrid) 
        {
            _manager.OnTickStart -= CreateNextPreview;
            _manager.OnTickStart += HideBullet;
        }
    }

    private void HideBullet(float timeToTickStart)
    {
        gameObject.SetActive(false);
        _manager.OnTickStart -= HideBullet;
    }

    public override Sprite GetPreviewSprite()
    {
        return GetComponentInChildren<SpriteRenderer>().sprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !isFriendly)
        {
            print("Destoryed!");
        }
        else if (collision.tag == "Enemy" && isFriendly)
        {
            print("Destoryed Enemy!");
        }
    }
}
