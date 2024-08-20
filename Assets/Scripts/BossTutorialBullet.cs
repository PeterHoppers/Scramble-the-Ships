using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossTutorialBullet : GridMovable
{
    public InputValue inputValue;
    public float cutsceneDuration = 2f;
    public CutsceneType triggeredCutscene = CutsceneType.Tutorial;

    private void Awake()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.Undestroyable;
    }

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

        _manager.ActivateCutscene(triggeredCutscene, cutsceneDuration);
    }
}
