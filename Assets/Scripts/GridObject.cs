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
    protected SpawnSystem _spawnSystem;

    public virtual void SetupObject(GameManager manager, SpawnSystem system, Tile startingTile)
    {
        _manager = manager;
        _spawnSystem = system;
        CurrentTile = startingTile;
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
        _spawnSystem.DespawnObject(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PerformInteraction(collision);
    }

    protected virtual void PerformInteraction(Collider2D collision)
    {

    }
}
