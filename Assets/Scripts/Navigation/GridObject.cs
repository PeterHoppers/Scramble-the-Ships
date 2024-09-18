using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LaserSystem2D;

public abstract class GridObject : MonoBehaviour, ILaserEntered
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
    protected ForeignCollisionStatus _foreignCollisionStatus = ForeignCollisionStatus.Default;

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

    public virtual bool CanBeShot()
    {
        return (_foreignCollisionStatus == ForeignCollisionStatus.Default);
    }

    public virtual bool IsIgnoredByBullets()
    {
        return (_foreignCollisionStatus == ForeignCollisionStatus.None);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<GridObject>(out var collidedGridObject))
        {
            return;
        }

        PerformInteraction(collidedGridObject);
    }

    protected virtual void PerformInteraction(GridObject collidedGridObject)
    {

    }

    public virtual void OnLaserEntered(LaserBase laserBase, List<RaycastHit2D> hits)
    {
        if (laserBase.transform.parent.TryGetComponent<GridObject>(out var gridObject))
        {
            gridObject.PerformInteraction(this);
        }
    }
}

public enum ForeignCollisionStatus
{ 
    Default,
    Undestroyable,
    None
}
