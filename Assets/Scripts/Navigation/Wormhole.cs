using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wormhole : GridMovable
{
    [SerializeField]
    private AudioClip _interactedSFX;

    private Wormhole _otherWormhole;
    private List<Previewable> _objectsInWormhole = new List<Previewable>();

    private void Start()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.None;
        var wormholes = transform.parent.GetComponentsInChildren<Wormhole>();

        foreach (var wormhole in wormholes)
        {
            if (wormhole != this && wormhole.isActiveAndEnabled)
            {
                _otherWormhole = wormhole;
                break;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Previewable>(out var objectExiting))
        {
            return;
        }

        PerformObjectLeave(objectExiting);
    }

    protected override void PerformInteraction(GridObject collidedGridObject)
    {
        base.PerformInteraction(collidedGridObject);

        if (!collidedGridObject.TryGetComponent<Previewable>(out var objectEntering))
        {
            return;
        }

        if (_objectsInWormhole.Contains(objectEntering))
        {
            return;
        }

        if (objectEntering.TryGetComponent<Fireable>(out var firable))
        {
            //if there's something in the wormhole, either let it collide with whatever is inside of it or let it leave as something inside of it is firing
            if (_objectsInWormhole.Count > 0)
            {
                firable.CollideWith(_objectsInWormhole[0]);
                return;
            }
        }

        _objectsInWormhole.Add(objectEntering);
        if (collidedGridObject.TryGetComponent<Player>(out var player))
        {
            GlobalAudioManager.Instance.PlayAudioSFX(_interactedSFX);
        }
        _otherWormhole.PassObjectThroughWormhole(objectEntering);
    }

    protected void PerformObjectLeave(Previewable objectLeaving)
    {
        _objectsInWormhole.Remove(objectLeaving);
    }

    public void PassObjectThroughWormhole(Previewable objectEntering)
    {
        _objectsInWormhole.Add(objectEntering);
        var timeUntilNextTick = _manager.GetMsUntilNextTick();
       
        var rotationQuaterion = new Quaternion();
        rotationQuaterion.eulerAngles = new Vector3(0, 0, 0);

        objectEntering.TransitionToRotation(rotationQuaterion, timeUntilNextTick / 3);
        StartCoroutine(objectEntering.WarpToTile(CurrentTile, timeUntilNextTick));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
