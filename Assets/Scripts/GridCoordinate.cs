using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCoordinate
{
    public Coordinate x;
    public Coordinate y;
}

[System.Serializable]
public class Coordinate
{
    public Anchor anchor;
    public int offset;

    public int GetIndexFromMax(float maxValue)
    { 
        return GetIndexFromMax((int)maxValue);
    }

    public int GetIndexFromMax(int maxValue)
    {
        int edgeBuffer = 1;
        switch (anchor)
        {
            case Anchor.End:
                if (offset < 0)
                {
                    return maxValue;
                }
                return maxValue - offset - edgeBuffer;
            case Anchor.Center:
                return (maxValue / 2) + offset;
            case Anchor.Start:
            default:
                if (offset < 0)
                {
                    return edgeBuffer;
                }
                return offset + edgeBuffer;
        }
    }
}

public enum Anchor
{ 
    Start,
    Center,
    End
}
