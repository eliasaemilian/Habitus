using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Grid
{
    public int rows;
    public int columns;
    public Cell center;
    public Cell[] Cells;

    public Vector3[] GridPoints;
    public Vector3[] GridVerts;
    public Vector3[] GridVertsOut;
    public int[] GridIndices;
}

[Serializable]
public class Cell
{
    public Vector2 ColRow;
    public Vector3 WorldPos;
    public bool ElevatedOnZ;
}

public class GridMaster : MonoBehaviour
{
    public Grid Grid;

    [SerializeField] private GameObject _tile = null;

    private float _tileWidth;
    private float _tileHeight;



    void Awake()
    {
        // TODO:
        SetCellSize();
        // Init Hexagonal Grid with row, column vars
        InitGrid();

        GetGridVerts();


        /*
         * Define Hexagon
        // each tile saves:
            // its pos in grid
            // its current type/color
            // its neighbour tiles

        // IDEAS:

        /*
         * Place Models on Hexagons
         * Grid Snapping for placing models on hexagons
         * Biome Logic: eg. Hexagons will init as lake/ water, hills etc in sensible pattern
         * Water tiles -> rivers form from spring to edge of tiles -> fall as waterfall
         */
    }

    void InitGrid()
    {
        Grid.center = new Cell();
        Grid.center.ColRow = new Vector2( 0, 0 );
        Grid.center.WorldPos = GetWorldPos( Grid.center.WorldPos, out Grid.center.ElevatedOnZ );

        Grid.Cells = new Cell[Grid.columns * Grid.rows];
        Grid.GridPoints = new Vector3[Grid.columns * Grid.rows];
        int i = 0;
        for ( int y = 0; y < ( Grid.columns ); y++ )
        {
            for ( int x = 0; x < ( Grid.rows ); x++ )
            {
                // todo
                // get points for each grid point here -DONE
                // get hexagon vertices points from grid points (ignore doubles/ redundant points) - DONE

                // then
                // write CB wit computeshader-> linestrip from points
                Cell c = new Cell();
                c.ColRow = new Vector2( x, y );
                c.WorldPos = GetWorldPos( c.ColRow, out c.ElevatedOnZ );

                GameObject debug = Instantiate( _tile );
                debug.name = "Tile " + c.ColRow;
                debug.transform.position = c.WorldPos;

                Grid.Cells[i] = c;
                Grid.GridPoints[i] = c.WorldPos;


                i++;
            }
        }
    }

    private Vector3 GetWorldPos( Vector2 gridPos, out bool elevatedOnZ )
    {
        float s = 0;

        if ( gridPos.x % 2 != 0 ) s = 1;

        float x = Grid.center.WorldPos.x + gridPos.x * ( ( _tileWidth / 2 ) + ( _tileWidth / 4 ) );
        float z = Grid.center.WorldPos.z + ( s + gridPos.y * 2 ) * ( _tileHeight / 2 );
        elevatedOnZ = s != 0 ? true : false;
        return new Vector3( x, 0, z );
    }

    void SetCellSize()
    {
        GameObject readBoundsTile = Instantiate( _tile );
        var bounds = readBoundsTile.GetComponentInChildren<Collider>().bounds;
        DestroyImmediate( readBoundsTile );
        _tileWidth = bounds.size.x;
        _tileHeight = bounds.size.z;
    }


    private void GetGridVerts()
    {
        if ( Grid.GridPoints.Length <= 0 ) return;

        // vertices
        List<int> indices = new List<int>();
        List<Vector3> verts = new List<Vector3>();
        float r = _tileWidth / 2;
        for ( int i = 0; i < Grid.GridPoints.Length; i++ )
        {
            Vector3 c = new Vector3( Grid.GridPoints[i].x, 0, Grid.GridPoints[i].z ); // center if tile

            Vector3 p1 = new Vector3( c.x + r, 0, c.z ); // 0
            Vector3 p2 = new Vector3( c.x + r * .5f, 0, c.z - r ); // 1
            Vector3 p3 = new Vector3( c.x - r * .5f, 0, c.z - r ); // 2
            Vector3 p4 = new Vector3( c.x - r, 0, c.z ); // 3
            Vector3 p5 = new Vector3( c.x - r * .5f, 0, c.z + r ); // 4
            Vector3 p6 = new Vector3( c.x + r * .5f, 0, c.z + r ); // 5

            Cell cell = Grid.Cells[i];

            // vertices
            if ( cell.ColRow.y == 0 ) // ROW 0
            {
                if (cell.ColRow.x == 0)
                {
                    verts.Add( p1 );
                    verts.Add( p2 );
                    verts.Add( p3 );
                    verts.Add( p4);
                    verts.Add( p5 );
                    verts.Add( p6 );
                }
                else
                {
                    verts.Add( p1 );
                    verts.Add( p2 );

                    if ( cell.ColRow.x % 2 != 0 )
                    {
                        verts.Add( p5 );
                    }
                    else verts.Add( p3 );

                    verts.Add( p6 );
                }
            }
            else // all ROWS 1 TO END
            {
                if ( cell.ColRow.x == 0 ) // COLUMN 0
                {
                    verts.Add( p4 );
                    verts.Add( p5 );
                }

                else // ROWS 1 TO END
                {
                    // Column Even
                    if ( cell.ColRow.x % 2 != 0 )
                    {
                        verts.Add( p1 );
                        verts.Add( p5 );
                    }
                    if ( cell.ColRow.x == Grid.rows - 1 ) verts.Add( p1 );
                }

                verts.Add( p6 );

            }

           
        }

        int totalIndices = 12 * ( Grid.columns * Grid.rows );

        for ( int i = 0; i < Grid.GridPoints.Length; i++ )
        {
            Cell cell = Grid.Cells[i];

            // indices
            if ( cell.ColRow.y == 0 )
            {
                if ( cell.ColRow.x == 0 )
                {
                    indices.Add( 0 );
                    indices.Add( 1 );
                    indices.Add( 1 );
                    indices.Add( 2 );
                    indices.Add( 2 );
                    indices.Add( 3 );
                    indices.Add( 3 );
                    indices.Add( 4 );
                    indices.Add( 4 );
                    indices.Add( 5 );
                    indices.Add( 5 );
                    indices.Add( 0 );
                }
                else if ( cell.ColRow.x % 2 != 0 ) // ODDS
                {
                    int current = 6;
                    if ( cell.ColRow.x > 1 ) current = 6 + ( 4 * ( (int)cell.ColRow.x - 1 ) );
                    int past = 0;
                    if ( cell.ColRow.x > 1 ) past = 6 + ( 4 * ( (int)( cell.ColRow.x - 2 ) ) );


                    indices.Add( current );
                    indices.Add( current + 1 );

                    indices.Add( current + 1 );
                    indices.Add( past );

                    indices.Add( past );
                    indices.Add( past + ( ( cell.ColRow.x > 1 ) ? 3 : 5 ) );

                    indices.Add( past + ( ( cell.ColRow.x > 1 ) ? 3 : 5 ) );
                    indices.Add( current + 2 );

                    indices.Add( current + 2 );
                    indices.Add( current + 3 );

                    indices.Add( current + 3 );
                    indices.Add( current );

                }
                else // EVENS
                {
                    int current = 6 + ( 4 * ( (int)cell.ColRow.x - 1 ) );
                    int past = 6 + ( 4 * ( (int)( cell.ColRow.x - 2 ) ) );

                    indices.Add( current );
                    indices.Add( current + 1 );

                    indices.Add( current + 1 );
                    indices.Add( current + 2 );

                    indices.Add( current + 2 );
                    indices.Add( past + 1 );

                    indices.Add( past + 1 );
                    indices.Add( past );

                    indices.Add( past );
                    indices.Add( current + 3 );

                    indices.Add( current + 3 );
                    indices.Add( current );

                }

            }
            else // ROW 1 & ABOVE
            {

                if ( cell.ColRow.x == 0 )
                {
                    int row0 = 6 + ( ( Grid.columns - 1 ) * 4 ) - 1;

                    int odd = 0;
                    if ( Grid.columns % 2 != 0 ) odd = 1;
                    int half = ( Grid.columns / 2 );
                    Debug.Log( "Row0 is " + row0 + " Half " + half );

                    int row = ( 3 * ( half + odd ) ) + ( 2 * half );
                    int current = row0 + ( (int)cell.ColRow.y * row );

                    indices.Add( current );
                    indices.Add( current );
                    indices.Add( current );
                    indices.Add( current );
                    indices.Add( current );

                    // HALP =(
                }
            }
        }
           


        Grid.GridIndices = indices.ToArray();
        Grid.GridVerts = verts.ToArray();
        Grid.GridVertsOut = new Vector3[Grid.GridIndices.Length];

        for ( int j = 0; j < Grid.GridIndices.Length; j ++ )
        {
            if ( Grid.GridIndices[j] < Grid.GridVerts.Length && Grid.GridIndices[j] >= 0 )
                Grid.GridVertsOut[j] = Grid.GridVerts[Grid.GridIndices[j]];
            else Debug.Log( " J IS " + j + " and above " + Grid.GridIndices.Length );
        }

    }


    
}
