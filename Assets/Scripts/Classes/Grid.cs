using System;
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

    public Matrix4x4 DebugWorldMatrix;




    // RASTER
    public Vector3[] CenterPoints;
    public Cell[] CellsQueued;
    public Vector3[] RasterVertices;


    // MESH BORDER
    private Vector3[] _borderVertices;
    public Vector3[] BorderVertices { get { return _borderVertices; } }
    private int[] _borderIndices;
    public int[] BorderIndices { get { return _borderIndices; } }

    // TERRAIN
    [SerializeField] private TerrainRenderer[] _terrainRenderers;

    public Grid( Config_Map config )
    {
        // GRID AND TILE INFORMATION
        _size = config.GridSize;
        _tileHeight = config.TileSize;
        _tileThickness = config.TileThickness;


        // DECLARE CENTER
        center = new Cell();
        center.ColRow = new Vector2Int( 0, 0 );
        center.WorldPos = new Vector3( 0, 0, 0 );


        // for grid raster
        CenterPoints = new Vector3[Size * Size];
        CellsQueued = new Cell[Size * Size];

        // for grid mesh
        Cells = new Cell[Size, Size];
        GridPoints = new Vector3[Size, Size];

        InitGridCells();

        // INIT TERRAINS
        InitTerrain( config );

        // Init Hexagons with Border
        InitHexagons();
    }

    private void InitGridCells()
    {
        int i = 0;
        for ( int y = 0; y < ( Size ); y++ ) // row
        {
            for ( int x = 0; x < ( Size ); x++ ) // col
            {
                Cell c = new Cell();
                c.ColRow = new Vector2Int( x, y );
                c.WorldPos = GetWorldPos( c.ColRow, out c.ElevatedOnZ );
                c.SetNeighbours( Size );

                c.Hexagon = new Hexagon(); // TODO wat

                Cells[x, y] = c;
                GridPoints[x, y] = c.WorldPos;
                CenterPoints[i] = c.WorldPos;
                CellsQueued[i] = c;

                i++;
            }
        }
    }

    private void InitTerrain( Config_Map config )
    {
        InitTerrainRenderers( config );
        _terrainTypes = MapGeneration.GenerateTerrainTypes( config );
    }

    public void InitTerrainRenderers( Config_Map config )
    {
        List<TerrainRenderer> terrainRenderers = new List<TerrainRenderer>();

        if (config.BlankTerrain != null ) terrainRenderers.Add( new TerrainRenderer( Size, TileHeight, TileWidth, config.BlankTerrain ) );
        if (config.MountainTerrain != null ) terrainRenderers.Add( new TerrainRenderer( Size, TileHeight, TileWidth, config.MountainTerrain ) );

        _terrainRenderers = terrainRenderers.ToArray();
    }

    private void InitHexagons()
    {
        List<Vector3> border = new List<Vector3>();
        for ( int i = 0; i < Cells.GetLength( 0 ); i++ )
        {
            for ( int j = 0; j < Cells.GetLength( 1 ); j++ )
            {
                Hexagon hex = new Hexagon();
                hex.center = new Vector3( Cells[i,j].WorldPos.x, Cells[i, j].WorldPos.y + TileThickness, Cells[i, j].WorldPos.z );
                AddHexagon( i, j, hex );

                // border.AddRange( Cells[i, j].GetBorderVerticesByNeighbour() );
            }
        }

        //_borderVertices = border.ToArray();
        //_borderIndices = new int[BorderVertices.Length];
        //for ( int i = 0; i < _borderIndices.Length; i++ ) _borderIndices[i] = i;
    }



    public void AddHexagon( int x, int y, Hexagon hex )
    {
        //Debug.Log($"Hex {hex.center} added to {_terrainTypes[x, y].ID}");
        GetTerrainRendererByID( _terrainTypes[x, y].ID ).AddHexagonToRenderer( x, y, hex );
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

    public void GenerateProceduralGrid()
    {
        for ( int i = 0; i < _terrainRenderers.Length; i++ )
        {
            _terrainRenderers[i].SetComputeBuffer();
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

    public void CleanUp()
    {
        if (_terrainRenderers != null)
        {
            for ( int i = 0; i < _terrainRenderers.Length; i++ )
            {
                _terrainRenderers[i].CleanUp();
            }
        }

    }

    public void DrawTerrainProcedural(ref UnityEngine.Rendering.CommandBuffer buf)
    {
        if ( _terrainRenderers == null ) return;

        for ( int i = 0; i < _terrainRenderers.Length; i++ )
        {
            _terrainRenderers[i].MatTerrain.SetPass( 0 );
            buf.DrawProcedural( DebugWorldMatrix, _terrainRenderers[i].MatTerrain, -1, MeshTopology.Triangles, _terrainRenderers[i].VerticesCount, 1 );

            Debug.Log( "Drawing Terrain " + _terrainRenderers[i].MatTerrain + " with " + _terrainRenderers[i].VerticesCount + " Vertices" );
        }
    }

    public TerrainRenderer GetTerrainRendererByID( int id )
    {
        for ( int i = 0; i < _terrainRenderers.Length; i++ )
        {
            if ( _terrainRenderers[i].GetID == id ) return _terrainRenderers[i];
        }

        Debug.LogError( $"No valid TerrainRenderer could be found for ID {id}" );
        return null;
    }

    private Vector3 GetWorldPos( Vector2 gridPos, out bool elevatedOnZ )
    {
        float s = 0;

        if ( gridPos.x % 2 != 0 ) s = 1;

        float x = center.WorldPos.x + gridPos.x * ( ( TileHeight / 2 ) + ( TileHeight / 4 ) );
        float z = center.WorldPos.z + ( s + gridPos.y * 2 ) * ( TileWidth / 2 );

        elevatedOnZ = s != 0 ? true : false;
        return new Vector3( x, 0, z );
    }
}
