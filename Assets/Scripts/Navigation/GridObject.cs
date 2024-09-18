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
    [SerializeField]
    protected ParticleSystem _deathSFX;

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
        if (_foreignCollisionStatus == ForeignCollisionStatus.Undestroyable)
        {
            return;
        }

        if (_deathSFX)
        {
            var deathEffect = Instantiate(_deathSFX, transform.parent);
            deathEffect.transform.position = new Vector3(transform.position.x, transform.position.y, 5); //TODO: Fix hard-coding, but this gets it rendering above the other objects
            deathEffect.Play();
        }

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

    public void OnLaserEntered(LaserBase laserBase, List<RaycastHit2D> hits)
    {
        var laserAdapter = laserBase.gameObject.transform.root.GetComponentInChildren<LaserAdapter>();

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
