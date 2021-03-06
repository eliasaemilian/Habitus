﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Grid {
    public Vector2[] SerializeData() {
        return _terrainRenderer.SerializeFromGPU();
    }

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
    public int[] GetTerrainTypeIDs {
        get {

            int[] types = new int[_terrainTypes.GetLength( 0 ) * _terrainTypes.GetLength( 1 )];
            int count = 0;
            for ( int i = 0; i < _terrainTypes.GetLength( 0 ); i++ ) {
                for ( int j = 0; j < _terrainTypes.GetLength( 1 ); j++ ) {
                    types[count] = _terrainTypes[i, j].ID;
                    count++;
                }
            }
            return types;
        }
    }

    public Matrix4x4 DebugWorldMatrix;

    private List<uint> _activeHexagons = new List<uint>();
    public List<uint> ActiveHexagons { get { return _activeHexagons; } set { _activeHexagons = value; } }


    // RASTER
    public Vector3[] CenterPoints;
    public Cell[] CellsQueued;
    public Vector3[] RasterVertices;




    // TERRAIN
    private TerrainRenderer _terrainRenderer;

    public Grid( Setup_Render setup, Config_Map config, TerrainType[,] types ) {
        // GRID AND TILE INFORMATION
        _size = config.GridSize;
        _tileHeight = config.TileSize;
        _tileThickness = config.TileThickness;


        // DECLARE CENTER
        center = new Cell {
            ColRow = new Vector2Int( 0, 0 ),
            WorldPos = new Vector3( 0, 0, 0 )
        };


        // for grid raster
        CenterPoints = new Vector3[Size * Size];
        CellsQueued = new Cell[Size * Size];

        // for grid mesh
        Cells = new Cell[Size, Size];
        GridPoints = new Vector3[Size, Size];


        InitGridCells();

        // INIT TERRAINS
        InitTerrainRenderer( setup );

        if ( types == null ) InitTerrain( config );
        else {
            _terrainTypes = types;
        }

        // Init Hexagons with Border
        InitHexagons();

        // Set current Active Hexagons
        SetActiveHexagonsForRender();

    }

    public uint GetHexIDByColRow( int col, int row ) {
        return (uint)( col + row * Size );
    }

    public void RemoveHexagon( uint[] ids ) {
        for ( int i = 0; i < ids.Length; i++ ) {
            if ( ActiveHexagons.Contains( ids[i] ) ) ActiveHexagons.Remove( ids[i] );
        }

        _terrainRenderer.RemoveHexagon( ids );
    }

    public void AddHexagon( uint[] ids ) {
        for ( int i = 0; i < ids.Length; i++ ) {
            if ( !ActiveHexagons.Contains( ids[i] ) ) ActiveHexagons.Add( ids[i] );
        }

        _terrainRenderer.AddHexagon( ids );
    }

    public void ChangeHexagon( uint[] ids, uint[] terrain ) {
        _terrainRenderer.ChangeHexagon( ids, terrain );
    }


    private void UpdateNeighbours() {
        for ( int i = 0; i < ActiveHexagons.Count; i++ ) {
            // TODO
        }
    }

    private void InitGridCells() {
        int i = 0;
        for ( int y = 0; y < ( Size ); y++ ) // row
        {
            for ( int x = 0; x < ( Size ); x++ ) // col
            {
                Cell c = new Cell();
                c.ColRow = new Vector2Int( x, y );
                c.WorldPos = GetWorldPos( c.ColRow, out c.ElevatedOnZ );
                c.SetNeighbours( Size );


                Cells[x, y] = c;
                GridPoints[x, y] = c.WorldPos;
                CenterPoints[i] = c.WorldPos;
                CellsQueued[i] = c;

                i++;
            }
        }
    }

    private void InitTerrain( Config_Map config ) {
        _terrainTypes = MapGeneration.GenerateTerrainTypes( config );
    }

    public void InitTerrainRenderer( Setup_Render setup ) {
        _terrainRenderer = new TerrainRenderer( Size, TileHeight, TileWidth, setup );
    }

    private void InitHexagons() {
        for ( int i = 0; i < Cells.GetLength( 0 ); i++ ) {
            for ( int j = 0; j < Cells.GetLength( 1 ); j++ ) {
                Vector3 center = new Vector3( Cells[i, j].WorldPos.x, Cells[i, j].WorldPos.y + TileThickness, Cells[i, j].WorldPos.z );
                Hexagon hex = new Hexagon( i, j, center, Cells[i, j].IsConnected );
                InitaddHexagon( i, j, hex );
                SetHexagonAsActive( i, j ); // FOR DEBUGGING ALL HEXAGONS ARE SET TO ACTIVE

            }
        }
    }


    public void SetHexagonAsActive( int x, int y ) {
        uint id = (uint)( x + y * Size );
        _activeHexagons.Add( id );
    }

    public void InitaddHexagon( int x, int y, Hexagon hex ) {
        hex.ChangeTerrainType( _terrainTypes[x, y] );
        if ( _terrainRenderer != null ) _terrainRenderer.AddHexagonToRenderer( x, y, hex );
    }

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

    public void GenerateProceduralGrid() {
        _terrainRenderer.ComputeVertexData();

    }

    public Cell[] GetNeighbourCellsByCell( Cell c ) {
        int size = 0;
        for ( int i = 0; i < c.IsConnected.Length; i++ ) {
            if ( c.IsConnected[i] ) size++;
        }

        List<Cell> cells = new List<Cell>();


        if ( c.IsConnected[0] ) cells.Add( Cells[c.ColRow.x, c.ColRow.y + 1] );
        if ( c.IsConnected[3] ) cells.Add( Cells[c.ColRow.x, c.ColRow.y - 1] );

        if ( c.ColRow.x % 2 != 0 ) // odd
        {
            if ( c.IsConnected[1] ) cells.Add( Cells[c.ColRow.x + 1, c.ColRow.y + 1] );
            if ( c.IsConnected[2] ) cells.Add( Cells[c.ColRow.x + 1, c.ColRow.y] );
            if ( c.IsConnected[4] ) cells.Add( Cells[c.ColRow.x - 1, c.ColRow.y] );
            if ( c.IsConnected[5] ) cells.Add( Cells[c.ColRow.x - 1, c.ColRow.y + 1] );

        } else // even
          {
            if ( c.IsConnected[1] ) cells.Add( Cells[c.ColRow.x + 1, c.ColRow.y] );
            if ( c.IsConnected[2] ) cells.Add( Cells[c.ColRow.x + 1, c.ColRow.y - 1] );
            if ( c.IsConnected[4] ) cells.Add( Cells[c.ColRow.x - 1, c.ColRow.y - 1] );
            if ( c.IsConnected[5] ) cells.Add( Cells[c.ColRow.x - 1, c.ColRow.y] );

        }

        return cells.ToArray();
    }

    public void CleanUp() {
        if ( _terrainRenderer != null ) _terrainRenderer.CleanUp();
    }

    public void DrawTerrainProcedural( ref UnityEngine.Rendering.CommandBuffer buf ) {
        if ( _terrainRenderer == null ) return;

        _terrainRenderer.MatTerrain.SetPass( 0 );
        buf.DrawProcedural( DebugWorldMatrix, _terrainRenderer.MatTerrain, -1, MeshTopology.Triangles, _terrainRenderer.VerticesCount, 1 );

        _terrainRenderer.Mat_Border.SetPass( 0 );
        buf.DrawProcedural( DebugWorldMatrix, _terrainRenderer.Mat_Border, -1, MeshTopology.Triangles, _terrainRenderer.VCountBorder, 1 );
    }

    private void SetActiveHexagonsForRender() {
        _terrainRenderer.ActiveHexagons = ActiveHexagons;
    }



    private Vector3 GetWorldPos( Vector2 gridPos, out bool elevatedOnZ ) {
        float s = 0;

        if ( gridPos.x % 2 != 0 ) s = 1;

        float x = center.WorldPos.x + gridPos.x * ( ( TileHeight / 2 ) + ( TileHeight / 4 ) );
        float z = center.WorldPos.z + ( s + gridPos.y * 2 ) * ( TileWidth / 2 );

        elevatedOnZ = s != 0;
        return new Vector3( x, 0, z );
    }
}
