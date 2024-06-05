using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class SpawnSystem : MonoBehaviour
{
    public float spawningDistance = 3;
    Dictionary<int, List<SpawnInfo>> _queuedSpawns = new Dictionary<int, List<SpawnInfo>>();

    GameManager _gameManager;
    int _ticksPassed;

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
    }

    public void OnTickEnd()
    {
        _queuedSpawns.TryGetValue(_ticksPassed, out var spawns);

        if (spawns != null)
        {
            foreach (var spawn in spawns)
            {
                if (spawn.spawnType == SpawnType.Preview)
                {
                    var previewable = spawn.objectToSpawn.GetComponent<Previewable>();
                    var previewAction = _gameManager.CreatePreviewOfPreviewableAtTile(previewable, spawn.tileToSpawnAt, 1, false);
                    _gameManager.AddPreviewAction(previewAction);
                }
                else if (spawn.spawnType == SpawnType.Player)
                {
                    var playerToSpawn = spawn.objectToSpawn.GetComponent<Player>();
                    playerToSpawn.OnSpawn();
                    playerToSpawn.SetPosition(spawn.tileToSpawnAt);
                }
                else
                {
                    var spawnedObject = SpawnObjectOffLevel(spawn.objectToSpawn, spawn.tileToSpawnAt, spawn.spawnRotation);

                    if (spawnedObject.TryGetComponent<GridMovable>(out var damageable))
                    {
                        damageable.SetupMoveable(_gameManager, spawn.tileToSpawnAt, spawningDistance);

                        if (damageable.TryGetComponent<EnemyShip>(out var enemyShip))
                        {
                            _gameManager.SendEnemyCommands(enemyShip, spawn.spawnExtraInfo);
                        }
                    }
                }
            }
        }

        _ticksPassed++;
    }

    public GameObject SpawnObjectAtTile(GameObject spawnObject, Tile spawnTile, Quaternion spawnRotation)
    {
        var spawnedObject = Instantiate(spawnObject, transform);
        spawnedObject.transform.localPosition = spawnTile.GetTilePosition();
        spawnedObject.transform.rotation = spawnRotation;
        return spawnedObject;
    }

    public GameObject SpawnObjectOffLevel(GameObject spawnObject, Tile spawnTile, Quaternion spawnRotation)
    {
        var spawnedObject = Instantiate(spawnObject, Vector2.zero, spawnRotation, transform);
        spawnedObject.transform.localPosition = spawnTile.GetTilePosition() + (-1 * spawningDistance * (Vector2)spawnedObject.transform.up);
        return spawnedObject;
    }

    Vector2 GetSpawnCoordinates(SpawnDirections spawnDirection, Coordinate coordinate)
    {
        var maxCoordinates = _gameManager.GetGridLimits();

        switch (spawnDirection)
        {
            case SpawnDirections.Left:
                return new Vector2(0, coordinate.GetIndexFromMax(maxCoordinates.y));
            case SpawnDirections.Right:
                return new Vector2(maxCoordinates.x, coordinate.GetIndexFromMax(maxCoordinates.y));
            case SpawnDirections.Top:
                return new Vector2(coordinate.GetIndexFromMax(maxCoordinates.x), maxCoordinates.y);
            case SpawnDirections.Bottom:
                return new Vector2(coordinate.GetIndexFromMax(maxCoordinates.x), 0);
            default:
                return Vector2.zero;
        }
    }

    Quaternion GetRotationFromSpawnDirection(SpawnDirections spawnDirection)
    {
        Quaternion rotation = Quaternion.identity;
        switch (spawnDirection)
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

    public void QueueEnemyToSpawn(EnemySpawn enemySpawn, int spawnTick)
    {
        var spawnCoordiantes = GetSpawnCoordinates(enemySpawn.spawnDirection, enemySpawn.otherCoordinate);
        var spawnPosition = _gameManager.GetByCoordinates(spawnCoordiantes);
        var spawnRotation = GetRotationFromSpawnDirection(enemySpawn.spawnDirection);

        var enemySpawnInfo = new SpawnInfo()
        {
            objectToSpawn = enemySpawn.enemyObject,
            tileToSpawnAt = spawnPosition,
            spawnRotation = spawnRotation,
            spawnType = SpawnType.Enemy,
            spawnExtraInfo = enemySpawn.commandId,
        };

        AddSpawnInfoAtTick(enemySpawnInfo, spawnTick);
    }

    public void QueuePlayerToSpawn(Player player, Tile playerSpawnTile, int spawnTick)
    {
        var playerSpawnPreview = new SpawnInfo()
        {
            objectToSpawn = player.gameObject,
            tileToSpawnAt = playerSpawnTile,
            spawnRotation = player.transform.rotation,
            spawnType = SpawnType.Preview
        };

        var playerSpawn = new SpawnInfo()
        {
            objectToSpawn = player.gameObject,
            tileToSpawnAt = playerSpawnTile,
            spawnRotation = player.transform.rotation,
            spawnType = SpawnType.Player
        };

        int spawnTickPreview = spawnTick - 1;
        AddSpawnInfoAtTick(playerSpawnPreview, spawnTickPreview);
        AddSpawnInfoAtTick(playerSpawn, spawnTick);
    }

    void AddSpawnInfoAtTick(SpawnInfo spawnInfo, int tick)
    {
        if (_queuedSpawns.TryGetValue(tick, out var spawns))
        {
            spawns.Add(spawnInfo);
        }
        else
        {
            _queuedSpawns.Add(tick, new List<SpawnInfo> { spawnInfo });
        }
    }
}

public struct PlayerSpawnInfo
{
    public int tickToSpawn;
    public Player playerToSpawn;
    public Tile spawnTile;
}

public struct SpawnInfo
{
    public GameObject objectToSpawn;
    public Tile tileToSpawnAt;
    public Quaternion spawnRotation;
    public SpawnType spawnType;
    public int spawnExtraInfo;
}

public enum SpawnType
{ 
    Default,
    Enemy,
    Player,
    Preview
}
