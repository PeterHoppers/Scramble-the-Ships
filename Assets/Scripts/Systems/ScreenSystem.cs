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
    GameManager _gameManager;
    int _screenAmount = 0;
    int _screensLoaded = 0;

    private void Awake()
    {       
        _gameManager = GetComponent<GameManager>();
    }

    public void SetScreens(Level level)
    {
        _levelScreens = level.levelScreens;
        _screenAmount = _levelScreens.Length;
    }

    public int GetScreensRemaining()
    {
        return _screenAmount - _screensLoaded;
    }

    public void SetupNewScreen(SpawnSystem spawnSystem, GridSystem gridSystem, EffectsSystem effectsSystem, DialogueSystem dialogueSystem)
    {
        var nextScreen = _levelScreens[_screensLoaded];

        spawnSystem.ClearObjects();
        SetScreenStarters(spawnSystem, gridSystem, nextScreen.startingItems);
        SetQueuedEnemies(spawnSystem, gridSystem, nextScreen.enemySpawnInformation);
        SetScreenTranistions(spawnSystem, gridSystem, screenTrigger, nextScreen.transitionGrids);

        foreach (var effect in nextScreen.effects)
        {
            effectsSystem.PerformEffect(effect);
        }

        dialogueSystem.SetDialogue(nextScreen.screenDialogue);
        
        _screensLoaded++;
    }

    public void ResetScreenGridObjects(SpawnSystem spawnSystem, GridSystem gridSystem)
    {
        var nextScreen = _levelScreens[_screensLoaded - 1];
        SetScreenStarters(spawnSystem, gridSystem, nextScreen.startingItems);
        SetQueuedEnemies(spawnSystem, gridSystem, nextScreen.enemySpawnInformation);
        SetScreenTranistions(spawnSystem, gridSystem, screenTrigger, nextScreen.transitionGrids);
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

    List<ScreenChangeTrigger> SetScreenTranistions(SpawnSystem spawnSystem, GridSystem gridSystem, ScreenChangeTrigger baseTrigger, List<GridCoordinate> transitionGrids)
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
