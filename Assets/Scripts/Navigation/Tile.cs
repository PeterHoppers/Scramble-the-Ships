using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2 gridCoordinates;
    private bool _isVisible;
    private TileType _tileType;
    private SpriteRenderer _spriteRenderer;
    private Color _defaultColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
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

            if (_tileType == TileType.Transition)
            {
                _spriteRenderer.color = Color.white;
                _spriteRenderer.sortingOrder = 1;
            }
            else
            {
                _spriteRenderer.color = _defaultColor;
                _spriteRenderer.sortingOrder = 0;

            }
        }
    }

    public Vector2 GetTilePosition()
    {
        return transform.localPosition;
    }

    public Vector2 GetTileCoordinates()
    {
        return gridCoordinates;
    }

    public string GetPrintableCoordinates()
    {
        return $"({gridCoordinates.x} , {gridCoordinates.y})";
    }
}

public enum TileType
{ 
    Default,
    Spawning,
    Transition
}
