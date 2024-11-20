using LaserSystem2D;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LaserAdapter : Fireable
{
    private AudioSource _laserAudioSource;
    [Space]
    [SerializeField]
    private Laser _firingLaser;
    [SerializeField]
    private Laser _previewLaser;
    [SerializeField]
    [Range(0f, 1f)]
    private float _fireDelay;

    private bool _isActiveLaser = false;

    private void Awake()
    {
        _foreignCollisionStatus = ForeignCollisionStatus.Undestroyable;
        _laserAudioSource = GetComponent<AudioSource>();
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

    protected override void CreateNextPreview(float timeToTickEnd, int currentTickNumber)
    {
        base.CreateNextPreview(timeToTickEnd, currentTickNumber);
        if (_isActiveLaser)
        {
            _previewLaser.gameObject.SetActive(true);
        }
    }

    void SetActiveState(bool isActive)
    {
        _isActiveLaser = isActive;
        _previewLaser.gameObject.SetActive(isActive);

        if (isActive)
        {
            _manager.OnTickEnd += EnableLasers;            
        }
        else
        {
            _manager.OnTickEnd += DisableLasers;            
        }
        
    }

    void EnableLasers(float tickEndDuration, int _)
    {
        _manager.OnTickEnd -= EnableLasers;
        StartCoroutine(DelayedFire(_fireDelay));
    }

    IEnumerator DelayedFire(float delay)
    { 
        yield return new WaitForSeconds(delay);
        _firingLaser.Enable();
        _laserAudioSource.Play();
    }

    void DisableLasers(float tickEndDuration, int _)
    {
        _manager.OnTickEnd -= DisableLasers;
        _firingLaser.Disable();
    }

    private void OnDisable()
    {
        _manager.OnTickEnd -= EnableLasers;
        _manager.OnTickEnd -= DisableLasers;
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
