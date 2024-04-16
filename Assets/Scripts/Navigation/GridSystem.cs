using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class GridSystem : MonoBehaviour
{
    public Tile tilePrefab;
    List<List<Tile>> _tiles = new List<List<Tile>>();
    private Grid _grid;

    public int gridWidth = 20;
    public int gridHeight = 10;

    void Start()
    {
        _grid = GetComponent<Grid>();
        CreateGrid();
    }

    public Tile GetPositionByCoordinate(int x, int y)
    {
        return _tiles[x][y];
    }

    public bool TryGetTileByCoordinates(float x, float y, out Tile tile)
    {
        int xIndex = (int)x;
        int yIndex = (int)y;

        if (xIndex >= 0 && xIndex < _tiles.Count && yIndex >= 0 && yIndex < _tiles[xIndex].Count)
        {
            tile = GetPositionByCoordinate(xIndex, yIndex);
            return true;
        }

        tile = null;
        return false;
    }

    private void CreateGrid()
    {
        var bounds = new Bounds();
        for (int widthIndex = 0; widthIndex < gridWidth; widthIndex++)
        {
            _tiles.Add(new List<Tile>());
            for (int heightIndex = 0; heightIndex < gridHeight; heightIndex++)
            {
                var worldPosition = _grid.GetCellCenterWorld(new Vector3Int(widthIndex, heightIndex));
                bounds.Encapsulate(worldPosition);
                var tile = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
                tile.gridCoordinates = new Vector2(widthIndex, heightIndex);
                _tiles[widthIndex].Add(tile);
            }
        }

        SetCamera(bounds);
    }

    private void SetCamera(Bounds bounds)
    {
        var _cam = Camera.main;
        bounds.Expand(1);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * _cam.pixelHeight / _cam.pixelWidth;

        _cam.transform.position = bounds.center + Vector3.back;
        _cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
    }
}
