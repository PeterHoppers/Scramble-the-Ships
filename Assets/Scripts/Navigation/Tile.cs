using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2 gridCoordinates;
    private bool _isVisible;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool IsVisible
    {
        get 
        { 
            return _isVisible;
        }

        set
        {
            _isVisible = value;
            _spriteRenderer.enabled = value;
        }
    }

    public Vector2 GetTilePosition()
    {
        return transform.localPosition;
    }
}
