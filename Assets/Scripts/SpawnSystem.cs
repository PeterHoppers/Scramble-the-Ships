using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class SpawnSystem : MonoBehaviour
{
    public float spawningDistance = 3;
    [Range(0f, 1f)]
    public float spawningPercentageOffscreen = .25f;
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
                        damageable.SetupMoveable(_gameManager, this, spawn.tileToSpawnAt);

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
        var offscreenPosition = GetOffscreenPosition(spawnedObject.transform.up, spawnTile.GetTilePosition(), true);
        spawnedObject.transform.localPosition = offscreenPosition;

        //the issue is that spawnedObject.transform.up handles which direction they should be spawning from
        return spawnedObject;
    }

    public Vector2 GetOffscreenPosition(Vector3 facingDirection, Vector2 currentPosition, bool isArriving)
    {
        var camera = Camera.main;
        var offscreenPercentage = 1 + spawningPercentageOffscreen;
        var bottomLeftPosition = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        var topRightPosition = camera.ViewportToWorldPoint(new Vector3(offscreenPercentage, offscreenPercentage, camera.nearClipPlane));

        var targetDirection = (isArriving) ? facingDirection * -1 : facingDirection;

        if (targetDirection == Vector3.left)
        {
            return new Vector2(bottomLeftPosition.x, currentPosition.y);
        }
        else if (targetDirection == Vector3.right)
        {
            return new Vector2(topRightPosition.x, currentPosition.y);
        }
        else if (targetDirection == Vector3.up)
        {
            return new Vector2(currentPosition.x, topRightPosition.y);
        }
        else if (targetDirection == Vector3.down)
        {
            return new Vector2(currentPosition.x, bottomLeftPosition.x);
        }
        else
        {
            return Vector2.zero;
        }
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
