using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : GridMovable
{
    [SerializeField]
    private ShipInfo shipInfo;

    [SerializedDictionary]
    private SerializedDictionary<int, InputValue> _shipCommands;
    private int _ticksSinceSpawn = 0;
    private int _commandsLoopAtTick = 0;

    private Fireable _currentFirable;

    public void SetCommands(SerializedDictionary<int, InputValue> commands, int commandsLoopAtTick)
    {
        _shipCommands = commands;
        _commandsLoopAtTick = commandsLoopAtTick;
    }

    protected override void CreateNextPreview(float timeToTickEnd)
    {
        if (_shipCommands != null && _shipCommands.TryGetValue(_ticksSinceSpawn, out var inputValue)) 
        {
            if (inputValue == InputValue.Fire)
            {
                if (shipInfo.fireable.TryGetComponent<Bullet>(out var bullet))
                {
                    var shootingTile = _manager.GetTileFromInput(this, inputValue);
                    var moveable = _manager.CreateMovableAtTile(bullet, this, shootingTile);
                    moveable.GetComponent<Bullet>().isFriendly = false;
                    moveable.GetComponentInChildren<SpriteRenderer>().sprite = shipInfo.bulletSprite;
                    var newPreview = _manager.CreatePreviewOfPreviewableAtTile(moveable, shootingTile);
                    newPreview.creatorOfPreview = this;
                    _manager.AddPreviewAction(newPreview);
                    movingInput = InputValue.None;
                }
                else
                {
                    if (_currentFirable == null)
                    {
                        var fireableGO = _spawnSystem.CreateToggableFirable(shipInfo.fireable, this);
                        _currentFirable = fireableGO.GetComponent<Fireable>();
                        _currentFirable.isFriendly = false;
                    }
                }
            }
            else
            {
                movingInput = inputValue;
            }

            if (_currentFirable != null) 
            {
                _currentFirable.OnOwnerInputChange(this, _manager, inputValue);
            }
        }

        base.CreateNextPreview(timeToTickEnd);
        _ticksSinceSpawn++;

        if (_commandsLoopAtTick > 0 && _ticksSinceSpawn >= _commandsLoopAtTick)
        {
            _ticksSinceSpawn = 0;
        }
    }

    public override void DestroyObject()
    {
        SetDeathSFX(shipInfo.deathVFX);
        base.DestroyObject();
    }

    protected override void PerformInteraction(GridObject collidedGridObject)
    {
        base.PerformInteraction(collidedGridObject);

        if (collidedGridObject.TryGetComponent(out Player playerHit))
        {
            _manager.HandleGridObjectCollision(this, playerHit);
        }
    }
}
