using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using System;

public class WaveManager : MonoBehaviour, IManager
{
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
            var previewAction = _gameManager.CreatePreviewAtPosition(playerInfo.playerToSpawn, playerInfo.spawnTile, false);
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
                var spawnPosition = _gameManager.GetByCoordinates(wave.spawnCoordiantes);
                var spawnedObject = Instantiate(wave.enemyObject, spawnPosition.GetTilePosition(), wave.enemyObject.transform.rotation, _gameManager.transform);

                if (spawnedObject.TryGetComponent<GridMovable>(out var damageable))
                {
                    damageable.SetupMoveable(_gameManager, spawnPosition);
                }
            }            
        }        

        ticksPassed++;
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
