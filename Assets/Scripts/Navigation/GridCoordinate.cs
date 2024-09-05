using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCoordinate
{
    public Coordinate x;
    public Coordinate y;

    public GridCoordinate(Coordinate x, Coordinate y)
    {
        this.x = x;
        this.y = y;
    }
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

    public Coordinate GetCoordinateFromOffset(int offset)
    {
        var newOffset = this.offset;
        var newCoordinate = new Coordinate()
        { 
            anchor = this.anchor
        };

        switch (this.anchor)
        {
            case Anchor.Start:
            case Anchor.End:
                newOffset += offset;
                break;
            case Anchor.Center:
                var isEvenOffset = (offset % 2) == 0;
                var dividedValue = Mathf.Ceil(offset / 2.0f);
                var adjustedOffset = Mathf.RoundToInt(dividedValue);
                newOffset += (isEvenOffset) ? adjustedOffset : -adjustedOffset;
                break;
            default:
                break;
        }

        newCoordinate.offset = newOffset;
        return newCoordinate;
    }
}

public enum Anchor
{ 
    Start,
    Center,
    End
}
