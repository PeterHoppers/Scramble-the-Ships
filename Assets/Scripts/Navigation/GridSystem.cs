using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public Tile tilePrefab;
    List<List<Tile>> _tiles = new List<List<Tile>>();
    private Grid _grid;

    public int gridWidth = 20;
    public int gridHeight = 10;
    public float boundsIncrease;
    int _titleSpawnDepth = 1; //how many rows/columns of tiles are dedicated to spawning

    void Start()
    {
        _grid = GetComponent<Grid>();
        CreateGrid();
    }

    public bool TryGetTileByCoordinates(GridCoordinate grid, out Tile tile)
    {
        return TryGetTileByCoordinates(grid.x, grid.y, out tile);
    }

    bool TryGetTileByCoordinates(Coordinate x, Coordinate y, out Tile tile)
    {
        int xIndex = x.GetIndexFromMax(GetMaxWidthIndex());
        int yIndex = y.GetIndexFromMax(GetMaxHeightIndex());
        return TryGetTile(xIndex, yIndex, out tile);
    }

    public bool TryGetTileByCoordinates(float x, float y, out Tile tile)
    {
        int xIndex = Mathf.RoundToInt(x);
        int yIndex = Mathf.RoundToInt(y);
        return TryGetTile(xIndex, yIndex, out tile);
    }

    bool TryGetTile(int xIndex, int yIndex, out Tile tile)
    {
        if (xIndex >= 0 && xIndex < _tiles.Count && yIndex >= 0 && yIndex < _tiles[xIndex].Count)
        {
            tile = GetPositionByCoordinate(xIndex, yIndex);
            return true;
        }

        tile = null;
        return false;
    }

    Tile GetPositionByCoordinate(int x, int y)
    {
        return _tiles[x][y];
    }

    public int GetMaxHeightIndex()
    { 
        return gridHeight - _titleSpawnDepth;
    }

    public int GetMaxWidthIndex() 
    { 
        return gridWidth - _titleSpawnDepth;
    }

    public Vector2 GetGridLimits()
    {
        return new Vector2(GetMaxWidthIndex(), GetMaxHeightIndex());
    }

    private void CreateGrid()
    {
        var bounds = new Bounds();
        for (int widthIndex = 0; widthIndex < gridWidth; widthIndex++)
        {
            var rowTiles = new List<Tile>();
            for (int heightIndex = 0; heightIndex < gridHeight; heightIndex++)
            {
                var worldPosition = _grid.GetCellCenterWorld(new Vector3Int(widthIndex, heightIndex));
                bounds.Encapsulate(worldPosition);
                var tile = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
                tile.gridCoordinates = new Vector2(widthIndex, heightIndex);
                tile.name = $"Tile ({widthIndex}, {heightIndex})";
                //set a property for the tiles around the outer edge to allow objects that attempt to enter them to know they are leaving the grid
                var isSpawning = (widthIndex == 0 || widthIndex == GetMaxWidthIndex() || heightIndex == 0 || heightIndex == GetMaxHeightIndex());

                if (isSpawning)
                {
                    tile.TileType = TileType.Spawning;
                }
                else
                { 
                    tile.TileType = TileType.Default;
                }

                rowTiles.Add(tile);
            }

            _tiles.Add(rowTiles);
        }

        SetCamera(bounds);
    }

    private void SetCamera(Bounds bounds)
    {
        var _cam = Camera.main;
        bounds.Expand(boundsIncrease);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * _cam.pixelHeight / _cam.pixelWidth;

        _cam.transform.position = bounds.center + Vector3.back;
        _cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
    }
}
