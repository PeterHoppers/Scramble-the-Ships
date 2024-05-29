using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;

public class WaveManager : MonoBehaviour, IManager
{
    public float spawningDistance = 3;
    [SerializedDictionary("Tick #", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawn[]> waveInformation = new SerializedDictionary<int, EnemySpawn[]>();

    List<PlayerSpawnInfo> playerSpawns = new List<PlayerSpawnInfo>();

    GameManager _gameManager;
    int ticksPassed;
    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickStart += OnTickStart;
        _gameManager.OnTickEnd += OnTickEnd;
        _gameManager.OnPlayerDeath += AddPlayerToSpawn;
    }

    private void OnTickStart(float timeToTickEnd)
    {
        var playersToPreview = playerSpawns.FindAll(x => x.tickToSpawn == ticksPassed);
        foreach (var playerInfo in playersToPreview)
        {
            var previewAction = _gameManager.CreatePreviewOfPreviewableAtTile(playerInfo.playerToSpawn, playerInfo.spawnTile, false);
            _gameManager.AddPreviewAction(previewAction);
        }

        var playersToSpawn = playerSpawns.FindAll(x => x.tickToSpawn + 1 == ticksPassed);

        foreach (var playerInfo in playersToSpawn)
        {
            playerInfo.playerToSpawn.OnSpawn();
            playerInfo.playerToSpawn.SetPosition(playerInfo.spawnTile);
        }
    }

    public void OnTickEnd(float tickDuration)
    {  
        waveInformation.TryGetValue(ticksPassed, out var waveInfo);

        if (waveInfo != null)
        { 
            foreach(var wave in waveInfo) 
            {
                var spawnCoordiantes = GetSpawnCoordinates(wave.spawnDirection, wave.otherCoordinate);
                var spawnPosition = _gameManager.GetByCoordinates(spawnCoordiantes);
                var spawnRotation = GetRotationFromSpawnDirection(wave.spawnDirection);
                var spawnedObject = Instantiate(wave.enemyObject, Vector2.zero, spawnRotation, _gameManager.transform);
                spawnedObject.transform.localPosition = spawnPosition.GetTilePosition() + (-1 * spawningDistance * (Vector2)spawnedObject.transform.up);

                if (spawnedObject.TryGetComponent<GridMovable>(out var damageable))
                {
                    damageable.SetupMoveable(_gameManager, spawnPosition, spawningDistance);

                    if (damageable.TryGetComponent<EnemyShip>(out var enemyShip))
                    {
                        enemyShip.shipCommands = wave.commands.shipCommands;
                    }
                }
            }            
        }        

        ticksPassed++;
    }

    Vector2 GetSpawnCoordinates(SpawnDirections spawnDirection, int coordinateOffset)
    {
        var maxCoordinates = _gameManager.GetGridLimits();
        switch (spawnDirection)
        {
            case SpawnDirections.Left:
                return new Vector2(0, coordinateOffset);
            case SpawnDirections.Right:
                return new Vector2(maxCoordinates.x, coordinateOffset);
            case SpawnDirections.Top:
                return new Vector2(coordinateOffset, maxCoordinates.y);
            case SpawnDirections.Bottom:
                return new Vector2(coordinateOffset, 0);
            default:
                return Vector2.zero;
        }
    }

    Quaternion GetRotationFromSpawnDirection(SpawnDirections spawnDirection)
    { 
        Quaternion rotation = Quaternion.identity;
        switch(spawnDirection) 
        { 
            case SpawnDirections.Top:
                rotation.eulerAngles = new Vector3(0, 0, 180);
                break;
            case SpawnDirections.Left:
                rotation.eulerAngles = new Vector3(0, 0, 270);
                break;
            case SpawnDirections.Right:
                rotation.eulerAngles = new Vector3(0, 0, 90);
                break;
            case SpawnDirections.Bottom:
            default: //Bottom would be no rotation
                break;
        }

        return rotation;
    }

    public void AddPlayerToSpawn(int ticksUntilSpawn, Player player, Tile playerSpawnTile)
    {
        int tickToSpawn = ticksUntilSpawn + ticksPassed;
        playerSpawns.Add(new PlayerSpawnInfo()
        {
            tickToSpawn = tickToSpawn,
            playerToSpawn = player,
            spawnTile = playerSpawnTile
        });
    }
}

public struct PlayerSpawnInfo
{
    public int tickToSpawn;
    public Player playerToSpawn;
    public Tile spawnTile;
}
