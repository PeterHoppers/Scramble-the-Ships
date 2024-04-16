using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;

public class WaveManager : MonoBehaviour, IManager
{
    [SerializedDictionary("Tick Number", "Enemy Spawn Info")]
    public SerializedDictionary<int, EnemySpawnInfo> waveInformation = new SerializedDictionary<int, EnemySpawnInfo>();

    GameManager _gameManager;
    int ticksPassed;
    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _gameManager.OnTickEnd += OnTickEnd;
    }

    public void OnTickEnd(float tickDuration)
    {
        waveInformation.TryGetValue(ticksPassed, out var wave);

        if (wave != null)
        { 
            var spawnPosition = _gameManager.GetByCoordinates(wave.spawnCoordiantes);            
            var spawnedObject = Instantiate(wave.enemyObject, spawnPosition.GetTilePosition(), wave.enemyObject.transform.rotation, _gameManager.transform);

            if (spawnedObject.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.SetupBullet(_gameManager, spawnPosition);
            }
        }

        ticksPassed++;
    }
}
