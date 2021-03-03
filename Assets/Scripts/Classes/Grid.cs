using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Grid
{
    public int size;
    public Cell center;
    public Cell[,] Cells;

    public Vector3[,] GridPoints;
    public Vector3[] GridVerts;
    public Vector3[] GridVertsOut;
    public int[] GridIndices;

    // TILES
    public TileType[,] TileTypes;

    public Dictionary<Vector3, Vector2> CenterColRowDict; // Lookup for Center Vert Position, Col [x] , Row [y]


    // RASTER
    public Vector3[] CenterPoints;
    public Cell[] CellsQueued;


    // MESH BORDER
    public Vector3[] VerticesSides;
    public int[] SidesIndices;

    // TERRAIN
    public TerrainRenderer TestTerrainGreen;
    public TerrainRenderer TestTerrainMountain;

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

    public Cell[] GetNeighbourCellsByCell( Cell c )
    {
        int size = 0;
        for ( int i = 0; i < c.IsConnected.Length; i++ )
        {
            if ( c.IsConnected[i] ) size++;
        }

        List<Cell> cells = new List<Cell>();


        if ( c.IsConnected[0] ) cells.Add( Cells[c.ColRow.x, c.ColRow.y + 1 ] );
        if ( c.IsConnected[3] ) cells.Add( Cells[c.ColRow.x, c.ColRow.y - 1 ] );

        if (c.ColRow.x % 2 != 0) // odd
        {
            if ( c.IsConnected[1] ) cells.Add( Cells[c.ColRow.x + 1, c.ColRow.y + 1 ] );
            if ( c.IsConnected[2] ) cells.Add( Cells[c.ColRow.x + 1, c.ColRow.y ] );
            if ( c.IsConnected[4] ) cells.Add( Cells[c.ColRow.x - 1, c.ColRow.y ] );
            if ( c.IsConnected[5] ) cells.Add( Cells[c.ColRow.x - 1, c.ColRow.y + 1 ] );

        }
        else // even
        {
            if ( c.IsConnected[1] ) cells.Add( Cells[ c.ColRow.x + 1 , c.ColRow.y] );
            if ( c.IsConnected[2] ) cells.Add( Cells[ c.ColRow.x + 1, c.ColRow.y - 1] );
            if ( c.IsConnected[4] ) cells.Add( Cells[ c.ColRow.x - 1, c.ColRow.y - 1] );
            if ( c.IsConnected[5] ) cells.Add( Cells[ c.ColRow.x - 1, c.ColRow.y ] );

        }

        return cells.ToArray();
    }

}
