using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;

public class SpawnSystem : MonoBehaviour
{
    public float spawningDistance = 3;
    [Range(0f, 1f)]
    public float spawningPercentageOffscreen = .25f;
    Dictionary<int, List<SpawnInfo>> _queuedSpawns = new Dictionary<int, List<SpawnInfo>>();
    List<GameObject> _spawnList = new List<GameObject>();

    [Header("Commands")]
    [SerializedDictionary("Command Id", "Command SO")]
    public SerializedDictionary<SpawnCommand, GridObjectCommands> commandBank = new SerializedDictionary<SpawnCommand, GridObjectCommands>();

    GameManager _gameManager;

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _gameManager.OnTickEnd += OnTickEnd;
    }

    void OnTickEnd(int ticksPassed)
    {
        SpawnItemsForTick(ticksPassed);
    }

    void SpawnItemsForTick(int tickNumber)
    {
        _queuedSpawns.TryGetValue(tickNumber, out var spawns);

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
                    _gameManager.MovePlayerOnScreenToTile(playerToSpawn, spawn.tileToSpawnAt, 1.5f); //tick duration                    
                }
                else
                {
                    var spawnedObject = SpawnObjectOffLevel(spawn.objectToSpawn, spawn.tileToSpawnAt, spawn.spawnRotation);

                    if (spawnedObject.TryGetComponent<GridMovable>(out var damageable))
                    {
                        damageable.SetupMoveable(_gameManager, this, spawn.tileToSpawnAt);

                        if (damageable.TryGetComponent<EnemyShip>(out var enemyShip))
                        {
                            SetCommandsForSpawnCommand(enemyShip, spawn.spawnCommand);
                        }
                    }
                }
            }

            _queuedSpawns.Remove(tickNumber);
        }
    }

    public GameObject SpawnObjectAtTile(GameObject spawnObject, Tile spawnTile, Quaternion spawnRotation)
    {
        var spawnedObject = Instantiate(spawnObject, transform);
        spawnedObject.transform.localPosition = spawnTile.GetTilePosition();
        spawnedObject.transform.rotation = spawnRotation;
        _spawnList.Add(spawnedObject);
        return spawnedObject;
    }

    public GameObject SpawnObjectOffLevel(GameObject spawnObject, Tile spawnTile, Quaternion spawnRotation)
    {
        var spawnedObject = Instantiate(spawnObject, Vector2.zero, spawnRotation, transform);
        var offscreenPosition = GetOffscreenPosition(spawnedObject.transform.up, spawnTile.GetTilePosition(), true);
        spawnedObject.transform.localPosition = offscreenPosition;
        _spawnList.Add(spawnedObject);

        //the issue is that spawnedObject.transform.up handles which direction they should be spawning from
        return spawnedObject;
    }

    public void MovePreviewableOffScreenToPosition(Previewable preview, Vector3 direction, Vector2 currentPosition, float duration, bool isArriving = false)
    {
        var offscreenPosition = GetOffscreenPosition(direction, currentPosition, isArriving);
        preview.TransitionToPosition(offscreenPosition, duration);
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

    public void SetCommandsForSpawnCommand(EnemyShip enemy, SpawnCommand command)
    {
        if (command == SpawnCommand.None) 
        { 
            return; 
        }

        var commands = commandBank[command];
        enemy.SetCommands(commands.commands, commands.commandsLoopAtTick);
    }

    Vector2 GetSpawnCoordinates(GridSystem gridSystem, SpawnDirections spawnDirection, Coordinate coordinate)
    {
        var maxCoordinates = gridSystem.GetGridLimits();

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

    public Quaternion GetRotationFromSpawnDirection(SpawnDirections spawnDirection)
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

    public void QueueEnemyToSpawn(GridSystem gridSystem, EnemySpawn enemySpawn, int spawnTick)
    {
        if (spawnTick == 0)
        {
            Debug.LogWarning("Enemies can only be previewed at tick end, meaning that ships can't be spawn on the 0th tick. Consider either moving them to the first tick.");
            spawnTick = 1;
        }

        var spawnCoordiantes = GetSpawnCoordinates(gridSystem, enemySpawn.spawnDirection, enemySpawn.otherCoordinate);
        var isSpawnable = gridSystem.TryGetTileByCoordinates((int)spawnCoordiantes.x, (int)spawnCoordiantes.y, out var spawnPosition);
        var spawnRotation = GetRotationFromSpawnDirection(enemySpawn.spawnDirection);

        var enemySpawnInfo = new SpawnInfo()
        {
            objectToSpawn = enemySpawn.enemyObject,
            tileToSpawnAt = spawnPosition,
            spawnRotation = spawnRotation,
            spawnType = SpawnType.Enemy,
            spawnCommand = enemySpawn.spawnCommand,
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

    public void ClearObjects()
    {
        foreach (var item in _spawnList)
        {
            if (item == null) 
            { 
                continue;
            }
            DespawnObject(item);
        }
        _spawnList.Clear();
    }

    public void DespawnObject(GridObject gridObject)
    {
        DespawnObject(gridObject.gameObject); //we should eventually look into pooling
        //gridObject.gameObject.SetActive(false);
    }

    public void DespawnObject(GameObject gameObject)
    { 
        Destroy(gameObject);
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
public struct SpawnInfo
{
    public GameObject objectToSpawn;
    public Tile tileToSpawnAt;
    public Quaternion spawnRotation;
    public SpawnType spawnType;
    public SpawnCommand spawnCommand;
}

public enum SpawnType
{ 
    Default,
    Enemy,
    Player,
    Preview
}

public enum SpawnCommand
{ 
    None,
    BasicShoot,
    BasicMovement,
    LoopShoot
}
