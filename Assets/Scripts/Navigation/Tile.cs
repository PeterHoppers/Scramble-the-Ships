using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2 gridCoordinates;
    private bool _isVisible;
    private TileType _tileType;
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

    public TileType TileType
    {
        get
        {
            return _tileType;
        }

        set
        {
            _tileType = value;

            if (_tileType == TileType.Spawning)
            {
                IsVisible = false;
            }
            else
            {
                IsVisible = true;
            }
        }
    }

    public Vector2 GetTilePosition()
    {
        return transform.localPosition;
    }
}

public enum TileType
{ 
    Default,
    Spawning,
    Transition
}
