using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTutorialBullet : Bullet
{
    protected override void PerformInteraction(Collider2D collision)
    {
        if (!collision.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        _manager.ScreenChangeTriggered(playerCollided);
    }
}
