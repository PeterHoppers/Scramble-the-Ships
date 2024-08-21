using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;
using UnityEngine.Device;

public class ScreenSystem : MonoBehaviour
{
    public ScreenChangeTrigger screenTrigger;
    private Screen[] _levelScreens;
    private Level _level;
    GameManager _gameManager;
    Screen _currentScreen;
    int _playerAmount = 0;
    int _screenAmount = 0;
    int _screensLoaded = 0;

    private void Awake()
    {       
        _gameManager = GetComponent<GameManager>();
    }

    public void SetScreens(Level level, int playerAmount)
    {
        _level = level;
        _playerAmount = playerAmount;
        _levelScreens = level.GetLevelScreens(playerAmount);
        _screenAmount = _levelScreens.Length;
    }

    public void TriggerStartingEffects(EffectsSystem effectsSystem)
    {
        foreach (var effect in _level.startingEffects)
        {
            effectsSystem.PerformEffect(effect);
        }
    }

    public int GetScreensRemaining()
    {
        return _screenAmount - _screensLoaded;
    }

    public List<GridCoordinate> GetStartingPlayerPositions(int playerAmount)
    {
        if (_currentScreen == null)
        {
            _currentScreen = _levelScreens[0];
        }

        return _level.GetStartingPlayerPositions(playerAmount, _currentScreen);
    }

    public void SetupNewScreen(SpawnSystem spawnSystem, GridSystem gridSystem, EffectsSystem effectsSystem, DialogueSystem dialogueSystem)
    {
        _currentScreen = _levelScreens[_screensLoaded];

        var screenTransitions = _level.GetTransitionGridPositions(_currentScreen);
        spawnSystem.LoopTick = _currentScreen.spawnsLoopAtTick;

        spawnSystem.ResetSpawns();
        SetScreenStarters(spawnSystem, gridSystem, _currentScreen.startingItems);
        SetQueuedEnemies(spawnSystem, gridSystem, _currentScreen.enemySpawnInformation);
        SetScreenTransitions(spawnSystem, gridSystem, screenTrigger, screenTransitions);

        foreach (var effect in _currentScreen.effects)
        {
            effectsSystem.PerformEffect(effect);
        }

        dialogueSystem.SetDialogue(_currentScreen.GetDialogue(_playerAmount));
        
        _screensLoaded++;
    }

    public void ResetScreenGridObjects(SpawnSystem spawnSystem, GridSystem gridSystem)
    {
        var screenTransitions = _level.GetTransitionGridPositions(_currentScreen); 
        spawnSystem.ResetSpawns();
        SetScreenStarters(spawnSystem, gridSystem, _currentScreen.startingItems);
        SetQueuedEnemies(spawnSystem, gridSystem, _currentScreen.enemySpawnInformation);
        SetScreenTransitions(spawnSystem, gridSystem, screenTrigger, screenTransitions);
    }

    void SetScreenStarters(SpawnSystem spawnSystem, GridSystem gridSystem, List<ScreenSpawns> screenStarters)
    {
        foreach (var spawn in screenStarters)
        {
            if (spawn.gridObject == null)
            {
                Debug.LogError("There is a null object trying to be spawned.");
                continue;
            }

            if (gridSystem.TryGetTileByCoordinates(spawn.spawnCoordinates, out var spawnPosition))
            {
                var rotation = spawnSystem.GetRotationFromSpawnDirection(spawn.facingDirection);
                var spawnedObject = spawnSystem.SpawnObjectAtTile(spawn.gridObject.gameObject, spawnPosition, rotation);

                if (spawnedObject.TryGetComponent<GridMovable>(out var movable))
                {
                    movable.SetupMoveable(_gameManager, spawnSystem, spawnPosition);

                    if (movable.TryGetComponent<EnemyShip>(out var enemyShip))
                    {
                        spawnSystem.SetCommandsForSpawnCommand(enemyShip, spawn.spawnCommand);
                    }
                }
                else
                {
                    spawnedObject.GetComponent<GridObject>().SetupObject(_gameManager, spawnSystem, spawnPosition);
                }
            }
        }
    }

    void SetQueuedEnemies(SpawnSystem spawnSystem, GridSystem gridSystem, SerializedDictionary<int, EnemySpawn[]> screenWaves)
    {
        foreach (var screenPair in screenWaves)
        {
            var wave = screenPair.Value;

            foreach (var spawn in wave)
            {
                spawnSystem.QueueEnemyToSpawn(gridSystem, spawn, screenPair.Key);
            }
        }
    }

    List<ScreenChangeTrigger> SetScreenTransitions(SpawnSystem spawnSystem, GridSystem gridSystem, ScreenChangeTrigger baseTrigger, List<GridCoordinate> transitionGrids)
    {
        var screenTriggers = new List<ScreenChangeTrigger>();
        foreach (var transition in transitionGrids)
        {
            if (gridSystem.TryGetTileByCoordinates(transition, out var spawnPosition))
            {
                var spawnedObject = spawnSystem.SpawnObjectAtTile(baseTrigger.gameObject, spawnPosition, baseTrigger.transform.rotation);
                spawnedObject.GetComponent<GridObject>().SetupObject(_gameManager, spawnSystem, spawnPosition);
                var screenTrigger = spawnedObject.GetComponent<ScreenChangeTrigger>();
                screenTriggers.Add(screenTrigger);
            }
        }

        return screenTriggers;
    }
}
