using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VFXPausing : MonoBehaviour
{
    [SerializeField]
    private float _playbackSpeed = 2f;
    ParticleSystem _mainParticleSystem;
    ParticleSystem[] _particleSystems;
    GameManager _gameManager;

    private void OnEnable()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        if (_gameManager == null)
        {
            print("Couldn't find the game mananger");
            return;
        }

        _mainParticleSystem = GetComponent<ParticleSystem>();
        _gameManager.OnTickEnd += PlayVFX;
        _gameManager.OnTickStart += PauseVFX;
        _gameManager.OnScreenChange += DisableVFX;
        _gameManager.OnScreenResetStart += DisableVFX;

        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        _mainParticleSystem.Play();
    }

    private void OnDisable()
    {
        if (_gameManager == null)
        {
            return;
        }

        _gameManager.OnTickEnd -= PlayVFX;
        _gameManager.OnTickStart -= PauseVFX;
        _gameManager.OnScreenChange -= DisableVFX;
        _gameManager.OnScreenResetStart -= DisableVFX;
    }

    void PlayVFX(float tickEndDuration, int _)
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            var main = particleSystem.main;
            main.simulationSpeed = _playbackSpeed;
        }
    }

    void PauseVFX(float _)
    {
        PauseVFX();
    }

    void PauseVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            var main = particleSystem.main;
            main.simulationSpeed = 0;
        }
    }

    void DisableVFX(int nextScreenIndex, int maxScreens)
    {
        DisableVFX();
    }

    void DisableVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            particleSystem.Stop();
        }
    }

}
