using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;

public class WaveManager : MonoBehaviour, IManager
{
    [SerializedDictionary("Tick #", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawn[]> waveInformation = new SerializedDictionary<int, EnemySpawn[]>();

    GameManager _gameManager;
    int ticksPassed;
    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickEnd += OnTickEnd;
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

                if (spawnedObject.TryGetComponent<Bullet>(out var bullet))
                {
                    bullet.SetupBullet(_gameManager, spawnPosition);
                }
            }            
        }

        ticksPassed++;
    }
}
