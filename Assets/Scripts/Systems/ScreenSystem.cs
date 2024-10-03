using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;

public class ScreenSystem : MonoBehaviour
{
    public ScreenChangeTrigger screenTrigger;
    private Screen[] _levelScreens;
    private Level _level;
    GameManager _gameManager;
    Screen _currentScreen;
    int _playerAmount = 0;
    public int ScreenAmount { get; private set; }
    public int ScreensLoaded { get; private set; }

    private void Awake()
    {       
        _gameManager = GetComponent<GameManager>();
    }

    public void SetScreens(Level level, int playerAmount)
    {
        _level = level;
        _playerAmount = playerAmount;
        _levelScreens = level.GetLevelScreens(playerAmount);
        ScreenAmount = _levelScreens.Length;
        PlayLevelSong();
        UpdateLevelBackground();
    }

    public Screen GetCurrentScreen()
    { 
        return _currentScreen;
    }

    void UpdateLevelBackground()
    {
        if (_level == null)
        {
            return;
        }

        if (_level.levelBackground == null)
        {
            return;
        }

        if (Camera.main.TryGetComponent<PanningBackground>(out var panningBackground)) 
        {
            panningBackground.UpdateBackgroundImage(_level.levelBackground);
        }
    }

    public void PlayLevelSong()
    {
        if (_level == null)
        {
            return;
        }

        GlobalAudioManager.Instance.TransitionSongs(_level.levelSong);
    }

    public void TriggerStartingEffects(EffectsSystem effectsSystem)
    {
        foreach (var effect in _level.startingEffects)
        {
            effectsSystem.PerformEffect(effect);
        }
    }
    public List<GridCoordinate> GetStartingPlayerPositions(int playerAmount)
    {
        return GetStartingPlayerInfo(playerAmount).positions;
    }

    public SpawnDirections GetStartingPlayerRotation(int playerAmount)
    {
        return GetStartingPlayerInfo(playerAmount).direction;
    }

    PlayerTransitionInfo GetStartingPlayerInfo(int playerAmount)
    {
        if (_currentScreen == null)
        {
            _currentScreen = _levelScreens[0];
        }

        return _level.GetStartingPlayerInfo(playerAmount, _currentScreen);
    }

    public void SetupNewScreen(SpawnSystem spawnSystem, GridSystem gridSystem, EffectsSystem effectsSystem, DialogueSystem dialogueSystem)
    {
        _currentScreen = _levelScreens[ScreensLoaded];
        spawnSystem.LoopTick = _currentScreen.spawnsLoopAtTick;
        GlobalAudioManager.Instance.TransitionSongs(_currentScreen.screenMusicTransition);

        ConfigureCurrentScreen(spawnSystem, gridSystem, effectsSystem);

        dialogueSystem.SetDialogue(_currentScreen.GetDialogue(_playerAmount));
        
        ScreensLoaded++;
    }

    public void ConfigureCurrentScreen(SpawnSystem spawnSystem, GridSystem gridSystem, EffectsSystem effectsSystem)
    {
        var screenTransitions = _level.GetTransitionGridInfo(_currentScreen); 
        spawnSystem.ResetSpawns();
        SetScreenStarters(spawnSystem, gridSystem, _currentScreen.startingItems);
        SetQueuedEnemies(spawnSystem, gridSystem, _currentScreen.enemySpawnInformation);
        SetScreenTransitions(spawnSystem, gridSystem, screenTrigger, screenTransitions);

        foreach (var effect in _currentScreen.effects)
        {
            effectsSystem.PerformEffect(effect);
        }
    }

    void SetScreenStarters(SpawnSystem spawnSystem, GridSystem gridSystem, List<ScreenSpawns> screenStarters)
    {
        foreach (var spawn in screenStarters)
        {
            var baseSpawnInfo = spawn.spawnInfo;
            var spawnData = spawnSystem.GetDataFromSpawnObject(baseSpawnInfo);
            var rotation = GetRotationFromFacingDirection(baseSpawnInfo.direction);

            var numberToSpawn = baseSpawnInfo.groupingSize;
            if (numberToSpawn == 0)
            {
                numberToSpawn = 1;
            }

            //use the direction and coordinates to figure out how to make a "line" of this object
            //direction will determine if is horizontal or veritcal, i.e. moving along X or Y
            //that coordinate will then determine which direction it is at, either counting up, down, or every other

            //TODO: handle this differently, perhaps have a bool somewhere that indicates that this type is multiple spaces or not
            var doesGoAcrossMultipleSpaces = baseSpawnInfo.objectToSpawn == SpawnObject.BossLaser;
            var isHorizontal = IsDirectionHorizontal(baseSpawnInfo.direction);
            if (doesGoAcrossMultipleSpaces)
            { 
                isHorizontal = !isHorizontal;
            }
            var targetCoordiante = (isHorizontal) ? spawn.spawnCoordinates.x : spawn.spawnCoordinates.y;

            for (var index = 0; index < numberToSpawn; index++) 
            {
                var offsetCoordinate = targetCoordiante.GetCoordinateFromOffset(index);
                var xCoordinate = (isHorizontal) ? offsetCoordinate : spawn.spawnCoordinates.x;
                var yCoordinate = (!isHorizontal) ? offsetCoordinate : spawn.spawnCoordinates.y;

                if (gridSystem.TryGetTileByCoordinates(new GridCoordinate(xCoordinate, yCoordinate), out var spawnTile) && spawnTile.IsVisible)
                {
                    var spawnedObject = spawnSystem.CreateSpawnObject(spawnData.objectToSpawn.gameObject, spawnTile, rotation);
                    spawnSystem.ConfigureSpawnedObject(spawnedObject, spawnTile, spawnData.command);
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

    List<ScreenChangeTrigger> SetScreenTransitions(SpawnSystem spawnSystem, GridSystem gridSystem, ScreenChangeTrigger baseTrigger, PlayerTransitionInfo transitionInfo)
    {
        var screenTriggers = new List<ScreenChangeTrigger>();
        var transitionGrids = transitionInfo.positions;
        foreach (var transition in transitionGrids)
        {
            if (gridSystem.TryGetTileByCoordinates(transition, out var spawnPosition))
            {
                var spawnedObject = spawnSystem.CreateSpawnObject(baseTrigger.gameObject, spawnPosition, baseTrigger.transform.rotation);
                spawnedObject.GetComponent<GridObject>().SetupObject(_gameManager, spawnSystem, spawnPosition);
                var screenTrigger = spawnedObject.GetComponent<ScreenChangeTrigger>();
                screenTrigger.SetScreenTransitionDirection(transitionInfo.direction);
                screenTriggers.Add(screenTrigger);
            }
        }

        return screenTriggers;
    }

    //A Flipped version of GetRoationFromSpawn direction, due to just wanting to rotate the object to match that direction
    Quaternion GetRotationFromFacingDirection(SpawnDirections spawnDirection)
    {
        Quaternion rotation = Quaternion.identity;
        switch (spawnDirection)
        {
            case SpawnDirections.Top:
                rotation.eulerAngles = new Vector3(0, 0, 0);
                break;
            case SpawnDirections.Left:
                rotation.eulerAngles = new Vector3(0, 0, 90);
                break;
            case SpawnDirections.Right:
                rotation.eulerAngles = new Vector3(0, 0, 270);
                break;
            case SpawnDirections.Bottom:
            default:
                rotation.eulerAngles = new Vector3(0, 0, 180);
                break;
        }

        return rotation;
    }

    bool IsDirectionHorizontal(SpawnDirections spawnDirection)
    {
        return (spawnDirection == SpawnDirections.Top || spawnDirection == SpawnDirections.Bottom);
    }
}
