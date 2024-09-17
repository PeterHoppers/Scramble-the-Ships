using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossTutorialBullet : GridMovable
{
    public InputValue inputValue;
    public float cutsceneDuration = 2f;

    private void Awake()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.Undestroyable;
        movingInput = inputValue;
    }

    protected override void PerformInteraction(Collider2D collision)
    {
        if (!collision.TryGetComponent<Player>(out var playerCollided))
        {
            return;
        }

        _manager.ActivateCutscene(CutsceneType.Hacking, cutsceneDuration);
    }

    public override Color GetPreviewColor()
    {
        return new Color(.5f, .5f, .5f, .25f);
    }
}
