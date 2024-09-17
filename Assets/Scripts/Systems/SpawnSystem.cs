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
    public int LoopTick { private get; set; }
    private int _loopTickOffset;
    Dictionary<int, List<SpawnInfo>> _queuedSpawns = new Dictionary<int, List<SpawnInfo>>();
    List<GameObject> _spawnList = new List<GameObject>();

    [SerializedDictionary("Spawn Object", "Corresponding Object & Commands")]
    public SerializedDictionary<SpawnObject, SpawnData> spawnBank = new SerializedDictionary<SpawnObject, SpawnData>();

    GameManager _gameManager;

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _gameManager.OnTickEnd += OnTickEnd;
    }

    void OnTickEnd(int ticksPassed)
    {
        var tickToPull = ticksPassed - _loopTickOffset;
        SpawnItemsForTick(tickToPull);

        if (LoopTick != 0 && ticksPassed - _loopTickOffset >= LoopTick)
        {
            _loopTickOffset = ticksPassed;
        }
    }

    void SpawnItemsForTick(int tickNumber)
    {
        _queuedSpawns.TryGetValue(tickNumber, out var spawns);

        if (spawns != null)
        {
            foreach (var spawn in spawns)
            {                
                var spawnedObject = CreateSpawnObject(spawn.objectToSpawn.gameObject, spawn.tileToSpawnAt, spawn.spawnRotation, true);
                ConfigureSpawnedObject(spawnedObject, spawn.tileToSpawnAt, spawn.spawnCommand);
            }
        }
    }

    public GameObject CreateSpawnObject(GameObject spawnObject, Tile spawnTile, Quaternion spawnRotation, bool isOffscreen = false)
    {
        var spawnedObject = Instantiate(spawnObject, Vector2.zero, spawnRotation, transform);
        var tilePosition = spawnTile.GetTilePosition();
        spawnedObject.transform.localPosition = (isOffscreen) ? GetOffscreenPosition(spawnedObject.transform.up, tilePosition, true) : tilePosition;
        _spawnList.Add(spawnedObject);
        return spawnedObject;
    }

    public void ConfigureSpawnedObject(GameObject spawnedObject, Tile spawnTile, GridObjectCommands spawnCommand)
    {
        if (spawnedObject.TryGetComponent<GridMovable>(out var damageable))
        {
            damageable.SetupMoveable(_gameManager, this, spawnTile);

            if (damageable.TryGetComponent<EnemyShip>(out var enemyShip))
            {
                SetCommandsForSpawnCommand(enemyShip, spawnCommand);
            }
        }
        else
        {
            spawnedObject.GetComponent<GridObject>().SetupObject(_gameManager, this, spawnTile);
        }
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

    public SpawnData GetDataFromSpawnObject(ObjectSpawnInfo spawnInfo)
    { 
        return spawnBank[spawnInfo.objectToSpawn];
    }

    public void SetCommandsForSpawnCommand(EnemyShip enemy, GridObjectCommands command)
    {
        if (command == null) 
        { 
            return; 
        }

        enemy.SetCommands(command.commands, command.commandsLoopAtTick);
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

    public void QueueEnemyToSpawn(GridSystem gridSystem, EnemySpawn enemySpawn, int spawnTick)
    {
        if (spawnTick <= 1)
        {
            Debug.LogWarning("Enemies can only be previewed at tick end, meaning that ships can't be spawn on the 1st tick. Consider either moving them to the second tick.");
            spawnTick = 2;
        }

        var baseEnemyInfo = enemySpawn.spawnInfo;
        var enemyData = GetDataFromSpawnObject(baseEnemyInfo);

        var numberToSpawn = baseEnemyInfo.groupingSize;
        if (numberToSpawn == 0)
        {
            numberToSpawn = 1;
        }

        for (var index = 0; index < numberToSpawn; index++)
        {
            var offsetCoordinates = enemySpawn.otherCoordinate.GetCoordinateFromOffset(index);
            var spawnCoordiantes = GetSpawnCoordinates(gridSystem, baseEnemyInfo.direction, offsetCoordinates);
            if (gridSystem.TryGetTileByCoordinates(spawnCoordiantes, out var spawnPosition))
            {
                var spawnRotation = GetRotationFromSpawnDirection(baseEnemyInfo.direction);
                var enemySpawnInfo = new SpawnInfo()
                {
                    objectToSpawn = enemyData.objectToSpawn,
                    tileToSpawnAt = spawnPosition,
                    spawnRotation = spawnRotation,
                    spawnCommand = enemyData.command,
                };

                AddSpawnInfoAtTick(enemySpawnInfo, spawnTick - 1); //-1 so that the preview is the tick before and them spawning is at that tick
            }
        }        
    }

    public void ResetSpawns()
    { 
        ClearObjects();
        _loopTickOffset = 0;
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
        _queuedSpawns.Clear();
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

[System.Serializable]
public struct SpawnData
{
    public GridObject objectToSpawn;
    public GridObjectCommands command;
}

public struct SpawnInfo
{
    public GridObject objectToSpawn;
    public Tile tileToSpawnAt;
    public Quaternion spawnRotation;
    public GridObjectCommands spawnCommand;
}

public enum SpawnCommand
{ 
    None,
    BasicShoot,
    BasicMovement,
    LoopShoot,
    SlowLoopShoot
}

public enum SpawnObject
{
    Meteor = 0,
    Turret = 1,
    RotatingTurret = 7,
    MovingShip = 2,
    ShootingShip = 3,
    Boss = 4,
    BossLaser = 5,
    EnergyPickup = 6,
}

[System.Serializable]
public enum SpawnDirections
{
    Top,
    Bottom,
    Left,
    Right
}