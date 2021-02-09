using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public TileType Type;

    public GameObject RefGO;
}

public enum TileType
{
    undefined, // 0
    blank, // 1
    mountain,
    plane,
    forest,
    river,
    lake
}
