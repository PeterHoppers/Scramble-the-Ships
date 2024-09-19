using LaserSystem2D;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LaserAdapter : Fireable
{
    [SerializeField]
    private GameObject _laserHolder;
    [SerializeField]
    private Laser _firingLaser;
    [SerializeField]
    private Laser _previewLaser;

    private bool _isActiveLaser = false;

    private void Awake()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.Undestroyable;
    }

    private void OnDisable()
    {
        _manager.OnTickEnd -= HidePreviewAfterTick;
    }

    public override void SetupMoveable(GameManager manager, SpawnSystem spawnSystem, Tile startingTile)
    {
        base.SetupMoveable(manager, spawnSystem, startingTile);
        manager.OnTickEnd += HidePreviewAfterTick;
    }

    void HidePreviewAfterTick(int _)
    {
        _previewLaser.gameObject.SetActive(false);
    }

    public override void OnOwnerInputChange(GridMovable owner, GameManager gameManager, InputValue previewInput)
    {
        switch (previewInput) 
        { 
            case InputValue.None:
                SetActiveState(false);
                _previewLaser.transform.localRotation = Quaternion.Euler(0, 0, 0f);
                break;
            case InputValue.Fire:               
                SetActiveState(true);
                _previewLaser.transform.localRotation = Quaternion.Euler(0, 0, 0f);
                break;
            case InputValue.Clockwise:
                _previewLaser.transform.localRotation = Quaternion.Euler(0, 0, -90f);
                break;
            case InputValue.Counterclockwise:
                _previewLaser.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                break;
            default:
                break;
        }
    }

    protected override void CreateNextPreview(float timeToTickEnd)
    {
        base.CreateNextPreview(timeToTickEnd);
        if (_isActiveLaser)
        {
            _previewLaser.gameObject.SetActive(true);
        }
    }

    void SetActiveState(bool isActive)
    {
        _isActiveLaser = isActive;
        _previewLaser.gameObject.SetActive(isActive);
        _manager.OnTickEnd += ToggleFiringLaser;
        void ToggleFiringLaser(int _)
        {
            _manager.OnTickEnd -= ToggleFiringLaser;
            if (isActive)
            {
                _firingLaser.Enable();
            }
            else
            {
                _firingLaser.Disable();
            }
        }
    }

    protected new Quaternion ConvertInputValueToRotation(InputValue input)
    {
        var currentRotation = transform.rotation;
        switch (input)
        {
            case InputValue.Clockwise:
                return currentRotation *= Quaternion.Euler(0, 0, 90f);
            case InputValue.Counterclockwise:
                return currentRotation *= Quaternion.Euler(0, 0, -90f);
            default:
                return currentRotation *= Quaternion.Euler(0, 0, 180f);
        }
    }

    public void OnLaserHit(Laser attackingLaser, GridObject hitObject)
    {
        if (attackingLaser.Id == _previewLaser.Id)
        {
            return;
        }

        if (attackingLaser.transform.IsChildOf(hitObject.transform))
        {
            return;
        }

        base.PerformInteraction(hitObject);
    }
}
