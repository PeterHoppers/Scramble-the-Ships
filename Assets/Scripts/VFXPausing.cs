using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VFXPausing : MonoBehaviour
{
    [SerializeField]
    private float _playbackSpeed = 2f; 
    [SerializeField]
    private bool _isPlayer = false;
    [SerializeField]
    private bool _activeBetweenScenes = false;

    ParticleSystem _mainParticleSystem;
    ParticleSystem[] _particleSystems;
    GameManager _gameManager;

    GameState _previousState;

    private void OnEnable()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        if (_gameManager == null)
        {
            print("Couldn't find the game mananger");
            return;
        }

        _mainParticleSystem = GetComponent<ParticleSystem>();
        _gameManager.OnTickEnd += OnTickEnd;
        _gameManager.OnTickStart += PauseVFX;
        _gameManager.OnScreenChange += OnScreenChange;
        _gameManager.OnScreenResetStart += OnScreenReset;
        _gameManager.OnGameStateChanged += OnGameStateChanged;

        _particleSystems = GetComponentsInChildren<ParticleSystem>();
        _mainParticleSystem.Play();
    }

    private void OnDisable()
    {
        if (_gameManager == null)
        {
            return;
        }

        _gameManager.OnTickEnd -= OnTickEnd;
        _gameManager.OnTickStart -= PauseVFX;
        _gameManager.OnScreenChange -= OnScreenChange;
        _gameManager.OnScreenResetStart -= OnScreenReset;
        _gameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    void OnTickEnd(float tickEndDuration, int _)
    {
        ResumeVFX();
    }

    public void ResumeVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            var main = particleSystem.main;
            main.simulationSpeed = _playbackSpeed;
        }
    }

    void PauseVFX(float _, int currentTickNumber)
    {
        PauseVFX();
    }

    public void PauseVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            var main = particleSystem.main;
            main.simulationSpeed = 0;
        }
    }

    IEnumerator DelayedPause(float waitDuration)
    { 
        yield return new WaitForSeconds(waitDuration);
        PauseVFX();
    }

    void OnGameStateChanged(GameState newState)
    {
        //we should look into a better way of handling this
        if (_previousState == GameState.GameOver && newState == GameState.Resetting && _isPlayer) 
        {
            ResumeVFX();
        }
        else if (newState == GameState.Paused || newState == GameState.Dialogue || newState == GameState.Playing)
        {
            PauseVFX();
        }
        else if ((newState == GameState.Resetting || newState == GameState.GameOver) && _activeBetweenScenes)
        {
            StartCoroutine(DelayedPause(.15f));
        }
        else if (newState == GameState.Transition && _activeBetweenScenes && !_isPlayer)
        {
            StartCoroutine(DelayedPause(.15f));
        }
        else
        {
            ResumeVFX();
        }

        _previousState = newState;
    }

    void OnScreenChange(int nextScreenIndex, int maxScreens)
    {
        if (_activeBetweenScenes)
        {
            StopVFX();
            PlayVFX();
        }
        else
        { 
            StopVFX();
        }
    }

    void OnScreenReset()
    {
        if (_activeBetweenScenes)
        {
            PlayVFX();
        }
        else
        {
            StopVFX();
        }
    }

    void StopVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            particleSystem.Stop();
        }
    }

    void PlayVFX()
    {
        foreach (ParticleSystem particleSystem in _particleSystems)
        {
            particleSystem.Play();
        }
    }
}
