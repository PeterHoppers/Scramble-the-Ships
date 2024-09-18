using LaserSystem2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAdapter : Fireable
{
    [SerializeField]
    private GameObject _laserHolder;
    [SerializeField]
    private Laser _firingLaser;
    [SerializeField]
    private Laser _previewLaser;

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

    void SetActiveState(bool isActive)
    {
        _previewLaser.gameObject.SetActive(isActive);
        _manager.OnTickStart += DisableFiringLaser;
        void DisableFiringLaser(float _)
        {
            _manager.OnTickStart -= DisableFiringLaser;
            _firingLaser.gameObject.SetActive(isActive);
        }
    }

    private void OnDestroy()
    {
        
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
}
