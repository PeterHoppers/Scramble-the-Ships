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
    protected ParticleSystem _deathSFX;

    private const int Z_ABOVE_RENDER_VALUE = 5;  //TODO: Fix hard-coding, but this gets it rendering above the other objects

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

    public virtual void SetDeathSFX(ParticleSystem particleSystem)
    {
        _deathSFX = particleSystem;
    }

    public virtual void DestroyObject()
    {
        if (_deathSFX)
        {
            var deathEffect = Instantiate(_deathSFX, transform.parent);
            deathEffect.transform.position = new Vector3(transform.position.x, transform.position.y, Z_ABOVE_RENDER_VALUE);

            if (!deathEffect.TryGetComponent<VFXPausing>(out var vfxPause))
            {
                deathEffect.Play();
            }
        }

        StopAllCoroutines();
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

    public virtual bool CannotBeDestoryed()
    {
        return (_foreignCollisionStatus == ForeignCollisionStatus.Undestroyable);
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

    public void OnLaserEntered(LaserBase laserBase, List<RaycastHit2D> hits)
    {
        var laserAdapter = laserBase.gameObject.transform.parent.parent.GetComponentInChildren<LaserAdapter>(); //Eww, find a better way to assoicate a laser to a laser adapter

        if (laserAdapter == null) 
        {
            print("Laser collided, but found nothing");
            return;
        }

        laserAdapter.OnLaserHit(laserBase.GetComponent<Laser>(), this);
    }
}

public enum ForeignCollisionStatus
{ 
    Default,
    Undestroyable,
    None
}
