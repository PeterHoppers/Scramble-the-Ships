using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : GridMovable
{
    [SerializeField]
    private ShipInfo shipInfo;

    public int tickThatLoops = 0;

    [SerializedDictionary]
    public SerializedDictionary<int, InputValue> shipCommands;

    private int _ticksSinceSpawn = 0;

    protected override void CreateNextPreview(float timeToTickEnd)
    {
        if (shipCommands.TryGetValue(_ticksSinceSpawn, out var inputValue)) 
        {
            if (inputValue == InputValue.Fire)
            {
                var shootingTile = _manager.GetTileFromInput(this, inputValue);
                var firingDirection = ConvertInputValueToDirection(inputValue);
                var moveable = _manager.CreateMovableAtTile(shipInfo.bullet, this, shootingTile, firingDirection);
                var newPreview = _manager.CreatePreviewOfPreviewableAtTile(moveable, shootingTile);
                newPreview.isCreated = true;
                _manager.AddPreviewAction(newPreview);
                travelDirection = Vector2.zero;
            }
            else
            { 
                var newDirection = ConvertInputValueToDirection(inputValue);
                travelDirection = newDirection;
            }
        }

        base.CreateNextPreview(timeToTickEnd);
        _ticksSinceSpawn++;

        if (tickThatLoops > 0 && _ticksSinceSpawn >= tickThatLoops)
        {
            _ticksSinceSpawn = 0;
        }
    }

    public override void DestroyObject()
    {
        //TODO: Rework this to use pooling, for right now this will cause a memory leak
        var deathEffect = Instantiate(shipInfo.deathVFX, transform.parent);
        deathEffect.transform.position = transform.position;
        deathEffect.Play();
        base.DestroyObject();
    }

    protected override void PerformInteraction(Collider2D collision)
    {
        base.PerformInteraction(collision);

        if (collision.TryGetComponent(out Player playerHit))
        {
            _manager.HandleGridObjectCollision(this, playerHit);
        }
    }
}