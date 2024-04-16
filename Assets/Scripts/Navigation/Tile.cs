using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2 gridCoordinates;

    public Vector2 GetTilePosition()
    {
        return transform.position;
    }
}
