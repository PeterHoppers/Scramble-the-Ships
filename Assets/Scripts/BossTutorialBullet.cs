using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossTutorialBullet : GridMovable
{
    public InputValue inputValue;
    protected override void CreateNextPreview(float timeToTickEnd)
    {
        var newDirection = ConvertInputValueToDirection(inputValue);
        travelDirection = newDirection;

        base.CreateNextPreview(timeToTickEnd);
    }

    protected override void PerformInteraction(Collider2D collision)
    {
        if (!collision.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        _manager.ScreenChangeTriggered(playerCollided);
    }
}
