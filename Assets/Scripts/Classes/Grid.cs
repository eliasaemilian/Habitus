using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Grid
{
    public int size;
    //public int rows;
    //public int columns;
    public Cell center;
    public Cell[] Cells;

    public Vector3[] GridPoints;
    public Vector3[] GridVerts;
    public Vector3[] GridVertsOut;
    public int[] GridIndices;

    public TileType[,] TileTypes;


    public void InitTileTypes()
    {
        TileTypes = new TileType[size, size];
        int half = ( size - 1 ) / 2;
        if ( size % 2 != 0 ) // odd
        {
            TileTypes[half, half] = TileType.mountain;
            TileTypes[half + 1, half] = TileType.blank;
            TileTypes[half - 1, half] = TileType.blank;
            TileTypes[half, half + 1] = TileType.blank;
            TileTypes[half, half - 1] = TileType.blank;
            TileTypes[half - 1, half + 1] = TileType.blank;
            TileTypes[half - 1, half - 1] = TileType.blank;
        }
        else // even 
        {
            TileTypes[half, half] = TileType.blank;
            TileTypes[half + 1, half] = TileType.blank;
            TileTypes[half, half + 1] = TileType.blank;
        }


    }

}
