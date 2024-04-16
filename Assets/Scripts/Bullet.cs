using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Previewable
{
    public bool isFriendly = true;
    public Vector3 travelDirection = Vector3.up;
    Collider2D _bulletCollider;

    void Awake()
    {
        _bulletCollider = GetComponent<Collider2D>();
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
