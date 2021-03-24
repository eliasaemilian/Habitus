﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Grid
{
    private int _size;
    public int Size { get { return _size; } }

    public Cell center;
    public Cell[,] Cells;

    public Vector3[,] GridPoints;

    // TILES
    private float _tileHeight;
    public float TileHeight { get { return _tileHeight; } }
    public float TileWidth { get { return _tileHeight * 0.8660254f; } }

    private float _tileThickness;
    public float TileThickness { get { return _tileThickness; } }

    private TerrainType[,] _terrainTypes;


    public Grid(Config_Map config)
    {
        _size = config.GridSize;
        _tileHeight = config.TileSize;
        _tileThickness = config.TileThickness;

        _terrainTypes = MapGeneration.GenerateTerrainTypes( config );
    }


    // RASTER
    public Vector3[] CenterPoints;
    public Cell[] CellsQueued;
    public Vector3[] RasterVertices;


    // MESH BORDER
    public Vector3[] VerticesSides;
    public int[] SidesIndices;

    // TERRAIN
    public TerrainRenderer TestTerrainGreen;

    //public void InitTileTypes()
    //{
    //    TileTypes = new TileType[size, size];
    //    int half = ( size - 1 ) / 2;
    //    if ( size % 2 != 0 ) // odd
    //    {
    //        TileTypes[half, half] = TileType.mountain;
    //        TileTypes[half + 1, half] = TileType.blank;
    //        TileTypes[half - 1, half] = TileType.blank;
    //        TileTypes[half, half + 1] = TileType.blank;
    //        TileTypes[half, half - 1] = TileType.blank;
    //        TileTypes[half - 1, half + 1] = TileType.blank;
    //        TileTypes[half - 1, half - 1] = TileType.blank;
    //    }
    //    else // even 
    //    {
    //        TileTypes[half, half] = TileType.blank;
    //        TileTypes[half + 1, half] = TileType.blank;
    //        TileTypes[half, half + 1] = TileType.blank;
    //    }


    //}

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
