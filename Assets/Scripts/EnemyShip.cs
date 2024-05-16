using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : GridMovable
{
    [SerializeField]
    private ShipInfo shipInfo;

    [SerializedDictionary]
    public SerializedDictionary<int, InputValue> shipCommands;

    private int _ticksSinceSpawn = 0;

    protected override void CreateNextPreview(float timeToTickEnd)
    {
        if (shipCommands.TryGetValue(_ticksSinceSpawn, out var inputValue)) 
        {
            if (inputValue == InputValue.Shoot)
            {
                var shootingTile = _manager.GetTileFromInput(this, inputValue);
                var firingDirection = ConvertInputValueToDirection(inputValue);
                var newPreview = _manager.CreateMovablePreviewAtTile(shipInfo.bullet, this, shootingTile, firingDirection);
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
    }
}
