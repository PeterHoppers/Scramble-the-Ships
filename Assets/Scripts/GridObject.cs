using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridObject : MonoBehaviour
{    
    private Tile _currentTile;
    public Tile CurrentTile 
    { 
        get 
        { 
            return _currentTile; 
        }

        protected set 
        { 
            _currentTile = value;
        }    
    }

    protected GameManager _manager;

    public virtual void SetupObject(GameManager manager)
    {
        _manager = manager;
    }

    public virtual Vector2 GetGridCoordinates()
    {
        return CurrentTile.gridCoordinates;
    }

    //Used for setting a tile directly at a position
    public virtual void SetPosition(Tile directTile)
    {
        CurrentTile = directTile;
        transform.localPosition = CurrentTile.GetTilePosition();
    }

    public virtual void DestroyObject()
    { 
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PerformInteraction(collision);
    }

    protected virtual void PerformInteraction(Collider2D collision)
    {

    }
}
