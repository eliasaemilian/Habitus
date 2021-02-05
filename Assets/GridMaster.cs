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

        List<Vector3> verts = new List<Vector3>();
        float r = _tileWidth / 2;
        for ( int i = 0; i < Grid.GridPoints.Length; i++ )
        {
            Vector3 c = new Vector3( Grid.GridPoints[i].x, 0, Grid.GridPoints[i].z ); // center if tile

            Vector3 p1 = new Vector3( c.x + r, 0, c.z );
            Vector3 p2 = new Vector3( c.x + r * .5f, 0, c.z - r );
            Vector3 p3 = new Vector3( c.x - r * .5f, 0, c.z - r );
            Vector3 p4 = new Vector3( c.x - r, 0, c.z );
            Vector3 p5 = new Vector3( c.x - r * .5f, 0, c.z + r );
            Vector3 p6 = new Vector3( c.x + r * .5f, 0, c.z + r );

            Cell cell = Grid.Cells[i];


            if ( cell.ColRow.y == 0 ) // ROW 0
            {
                verts.Add( p1 );
                verts.Add( p2 );
                verts.Add( p6 );

                if ( cell.ColRow.x % 2 != 0 )
                {
                    verts.Add( p5 );
                }
                else verts.Add( p3 );

                if ( cell.ColRow.x == 0 )
                {
                    if ( !verts.Contains( p3 ) ) verts.Add( p3 );
                    verts.Add( p4 );
                    if ( !verts.Contains( p5 ) ) verts.Add( p5 );
                }

            }

            else // all ROWS 1 TO END
            {
                verts.Add( p6 );
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
                        verts.Add( p5 );
                        verts.Add( p1 );
                    }
                    if (cell.ColRow.x == Grid.rows - 1) verts.Add( p1 );
                }
            }
        }

        // INDICES
        List<int> indices = new List<int>();




        Grid.GridVerts = verts.ToArray();
    }


}
