﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMaster : MonoBehaviour
{
    public static GridMaster Instance;

    public Grid Grid;

    [SerializeField] private GameObject _tile = null;

    private float _tileWidth;
    private float _tileHeight;
    private float _tileThickness;

    public Material testMat, testMat2;
    public List<Vector3> TestVerts;
    public List<int> TestIndices;


    void Awake()
    {
        if ( Instance == null ) Instance = this;
        else Destroy( gameObject );
        //SetCellSize();
        //InitGrid(Grid.columns);

        //GetGridVerts();

        // IDEAS:

        /*
         * Place Models on Hexagons
         * Grid Snapping for placing models on hexagons
         * Biome Logic: eg. Hexagons will init as lake/ water, hills etc in sensible pattern
         * Water tiles -> rivers form from spring to edge of tiles -> fall as waterfall
         */
    }

    private void ClearGrid()
    {
        foreach ( Cell c in Grid.Cells )
        {
            if ( c.Tile.RefGO != null ) DestroyImmediate( c.Tile.RefGO ); 
        }
    }

    public void OnClickClearGrid() => ClearGrid();

    public void InitGrid(int size, TileType[,] tileTypes)
    {
        ClearGrid();
        SetCellSize();



        Grid = new Grid();
        Grid.size = size;


        Grid.center = new Cell();
        Grid.center.ColRow = new Vector2( 0, 0 );
        Grid.center.WorldPos = new Vector3( 0, 0, 0 );

        Grid.Cells = new Cell[Grid.size * Grid.size];
        Grid.GridPoints = new Vector3[Grid.size * Grid.size];
        Grid.TileTypes = tileTypes;

        //Grid.InitTileTypes();

        int i = 0;
        for ( int y = 0; y < ( Grid.size ); y++ )
        {
            for ( int x = 0; x < ( Grid.size ); x++ )
            {
                Cell c = new Cell();
                c.ColRow = new Vector2( x, y );
                c.WorldPos = GetWorldPos( c.ColRow, out c.ElevatedOnZ );

                if ( Grid.TileTypes[y, x] > 0 )
                {
                    c.Tile = new Tile();
                 //   c.Tile.RefGO = AddTileToCell( c, Grid.TileTypes[y, x] );
                }

                // AddTileToCell( c, TileType.blank );

                Grid.Cells[i] = c;
                Grid.GridPoints[i] = c.WorldPos;


                i++;
            }
        }

        GetGridVerts();
    }

    private Vector3 GetWorldPos( Vector2 gridPos, out bool elevatedOnZ )
    {
        float s = 0;

        if ( gridPos.x % 2 != 0 ) s = 1; 

        float x = Grid.center.WorldPos.x + gridPos.x * ( ( _tileHeight / 2 ) + ( _tileHeight / 4 ) );
        float z = Grid.center.WorldPos.z + ( s + gridPos.y * 2 ) * ( _tileWidth / 2 );

        elevatedOnZ = s != 0 ? true : false;
        return new Vector3( x, 0, z );
    }

    void SetCellSize()
    {
        GameObject readBoundsTile = Instantiate( _tile );
        var bounds = readBoundsTile.GetComponentInChildren<Collider>().bounds;
        DestroyImmediate( readBoundsTile );
        _tileWidth = 2;
        _tileHeight = 1.73f;

        _tileWidth = bounds.size.z;
        _tileHeight = bounds.size.x;
        _tileThickness = bounds.size.y;
    }


    private GameObject AddTileToCell( Cell c, TileType type )
    {
        GameObject debug = Instantiate( _tile );
        var mat = debug.GetComponentInChildren<MeshRenderer>().material;
        if ( type == TileType.mountain ) mat.color = Color.red;
        else if ( type == TileType.plane ) mat.color = Color.green;

        debug.name = "Tile " + c.ColRow;
        debug.transform.position = c.WorldPos;

        return debug;
    }

    private Vector3[] GenHexagonMeshInfo(Vector3 center, Vector3[] oVerts)
    {
        // verts
        Vector3[] verts = new Vector3[24 + 2];
        int[] otherIndices = new int[( 3 * 24 ) + 36];
        int[] topPlane = new int[3 * 12];
        int c = 1;
        Vector3 cTop = new Vector3( center.x, _tileThickness, center.z ); // index 13
        Vector3 cBot = center; // index 0
        verts[0] = cBot;
        verts[13] = cTop;
        for ( int i = 0; i < oVerts.Length; i++ )
        {
            verts[c] = new Vector3( oVerts[i].x, 0, oVerts[i].z );
            verts[c + 1] = new Vector3( oVerts[i].x, 0, oVerts[i].z );
            c += 2;
        }
        c = 14;
        for ( int i = 0; i < oVerts.Length; i++ )
        {
            verts[c] = oVerts[i];
            verts[c + 1] = oVerts[i];
            c += 2;
        }


        // indices
        c = 0;
        // bottom hex plane
        for ( int i = 1; i < 11; i += 2 )
        {
            otherIndices[c] = 0;
            otherIndices[c + 1] = i + 2;
            otherIndices[c + 2] = i;
            c += 3;
        }
        otherIndices[c] = 0;
        otherIndices[c + 1] = 1;
        otherIndices[c + 2] = 11;
        c += 3;


        // sides
        for ( int i = 0; i < 5; i ++ )
        {
            otherIndices[c] = 15 + ( i * 2 );
            otherIndices[c + 1] = ( i * 2 ) + 2 ;
            otherIndices[c + 2] = 15 + ( i * 2 ) + 2;

            otherIndices[c + 3] = ( i * 2 ) + 2;
            otherIndices[c + 4] = ( i * 2 ) + 4;
            otherIndices[c + 5] = 15 + ( i * 2 ) + 2;

            c += 6;
        }

        otherIndices[c] = 12;
        otherIndices[c + 1] = 15;
        otherIndices[c + 2] = 25;

        otherIndices[c + 3] = 15;
        otherIndices[c + 4] = 12;
        otherIndices[c + 5] = 2;


        // top hex plane
        c = 0;
        for ( int i = 14; i < 24; i += 2 )
        {
            topPlane[c] = i;
            topPlane[c + 1] = i + 2;
            topPlane[c + 2] = 13;
            c += 3;
        }
        topPlane[c] = 24;
        topPlane[c + 1] = 14;
        topPlane[c + 2] = 13;



        GameObject debug = new GameObject();
        debug.name = "test";
        MeshRenderer renderer = debug.AddComponent<MeshRenderer>();
        MeshFilter filter = debug.AddComponent<MeshFilter>();
        renderer.material = testMat;

        Mesh mesh = new Mesh();
        mesh.name = "Hexagon Mesh";
        mesh.subMeshCount = 2;
        UnityEngine.Rendering.SubMeshDescriptor desc = new UnityEngine.Rendering.SubMeshDescriptor();
        renderer.materials = new Material[2] { testMat, testMat2 };

        mesh.SetSubMesh( 1, desc, UnityEngine.Rendering.MeshUpdateFlags.Default );
        mesh.SetVertices( verts );
        mesh.SetIndices( otherIndices, MeshTopology.Triangles, 0 );
        mesh.SetIndices( topPlane, MeshTopology.Triangles, 1 );
        mesh.RecalculateNormals();
        filter.mesh = mesh;

        return verts;
    }


    private void GetGridVerts()
    {
        if ( Grid.GridPoints.Length <= 0 ) return;

        // vertices
        List<int> indices = new List<int>();
        List<Vector3> verts = new List<Vector3>();
        //float r = _tileWidth * .5f;

        float r = _tileHeight * .5f;
        float w = _tileWidth * .5f;

        for ( int i = 0; i < Grid.GridPoints.Length; i++ )
        {
            Vector3 c = new Vector3( Grid.GridPoints[i].x, 0, Grid.GridPoints[i].z ); // center of tile
            float y = _tileThickness;

            Vector3 p1 = new Vector3( c.x + r, y, c.z ); // 0
            Vector3 p2 = new Vector3( c.x + r * .5f, y, c.z - w ); // 1
            Vector3 p3 = new Vector3( c.x - r * .5f, y, c.z - w ); // 2
            Vector3 p4 = new Vector3( c.x - r, y, c.z ); // 3
            Vector3 p5 = new Vector3( c.x - r * .5f, y, c.z + w ); // 4
            Vector3 p6 = new Vector3( c.x + r * .5f, y, c.z + w ); // 5
           
            Cell cell = Grid.Cells[i];
            cell.Verts = GenHexagonMeshInfo( c, new Vector3[6] { p1, p2, p3, p4, p5, p6 } );
            List<Vector3> cverts = new List<Vector3>();
            // vertices
            if ( cell.ColRow.y == 0 ) // ROW 0
            {
                if (cell.ColRow.x == 0)
                {
                    cverts.Add( p1 );
                    cverts.Add( p2 );
                    cverts.Add( p3 );
                    cverts.Add( p4);
                    cverts.Add( p5 );
                    cverts.Add( p6 );
                }
                else
                {
                    cverts.Add( p1 );
                    cverts.Add( p2 );

                    if ( cell.ColRow.x % 2 != 0 )
                    {
                        cverts.Add( p5 );
                    }
                    else cverts.Add( p3 );

                    cverts.Add( p6 );
                }
            }
            else // all ROWS 1 TO END
            {
                if ( cell.ColRow.x == 0 ) // COLUMN 0
                {
                    cverts.Add( p4 );
                    cverts.Add( p5 );
                }

                else // ROWS 1 TO END
                {
                    // Column ODDS
                    if ( cell.ColRow.x % 2 != 0 )
                    {
                        cverts.Add( p1 );
                        cverts.Add( p5 );
                    }
                    else // Column EVEN
                    {
                        if ( cell.ColRow.x == Grid.size - 1 ) cverts.Add( p1 ); //repl. col
                    }
                  
                }

                cverts.Add( p6 );

            }

            verts.AddRange( cverts );
           
        }

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
            else  // ROW 1 & ABOVE
            {
                int row0 = 6 + ( ( Grid.size - 1 ) * 4 ); // repl col for all 3
                int half = ( Grid.size / 2 );
                int odd = ( Grid.size % 2 != 0 ) ? 1 : 0;

                if ( cell.ColRow.x == 0 )
                {

                    int row = 3 + ( 3 * ( half ) ) + ( half ) + 1;
                    if (odd == 0) row = 3 + ( 3 * ( half ) ) + ( half - 1 );

                    int current = row0 + ( ((int)cell.ColRow.y - 1 ) * row ); // first index newly added in this row


                    int pastRow = 6 + ( ( (int)cell.ColRow.x ) * 4 ) - 1;
                    if ( cell.ColRow.y == 1 ) pastRow = 0;

                    if (cell.ColRow.y > 1 ) pastRow = row0 + ( ( ( (int)cell.ColRow.y - 2 ) * row ) );

                    if (cell.ColRow.y != 1)
                    {
                        indices.Add( pastRow + 1 );
                        indices.Add( current );

                        indices.Add( current );
                        indices.Add( current + 1);

                        indices.Add( current + 1);
                        indices.Add( current + 2);

                        indices.Add( current + 2);
                        indices.Add( pastRow + 4);
                    }
                    else
                    {
                        indices.Add( pastRow + 8 );
                        indices.Add( pastRow + 5 );

                        indices.Add( pastRow + 4 );
                        indices.Add( current );

                        indices.Add( current );
                        indices.Add( current + 1 );

                        indices.Add( current + 1 );
                        indices.Add( current + 2 );

                        indices.Add( current + 2 );
                        indices.Add( pastRow + 8 );
                    }

                }
                else if ( cell.ColRow.x % 2 != 0 ) // ODDS 
                {

                    int curHalf = ( (int)cell.ColRow.x - 1  ) / 2;
                    
                    if ( curHalf < 0 ) curHalf = 0;

                    int thisRowAdded = 3 + ( 3 * curHalf) + ( curHalf ); 
                    if (cell.ColRow.x > 1) thisRowAdded = ( 3 * curHalf ) + ( curHalf + 1)  + 1; // 0 counts as even, add +1 since its 3 not 2 verts added


                    int row = 3 + ( 3 * ( half ) ) + ( half ) + 1;
                    if ( odd == 0 ) row = 3 + ( 3 * ( half ) ) + ( half - 1 );

                    int current = row0 + ( ( (int)cell.ColRow.y - 1 ) * row ) + thisRowAdded; // first index newly added in this row + everything added to this row - > + 1 this is where we are now
                    if ( cell.ColRow.x > 1 ) current += 1;

                    int pastRow = 6 + ( 4 * ( (int)cell.ColRow.x - 1 ) ); 
                    if ( cell.ColRow.y > 1) pastRow = (   Mathf.Clamp(( (int)cell.ColRow.y - 2 ), 0, (int)cell.ColRow.y) * ( row ) ) + thisRowAdded + row0;
                    if ( cell.ColRow.x > 1 ) pastRow += 1;

                    if ( cell.ColRow.x == 1 && cell.ColRow.y == 1 )
                    {
                        indices.Add( current );
                        indices.Add( pastRow + 3 );
                    }
                    else
                    {
                        indices.Add( current );
                        indices.Add( pastRow + 2 );
                    }

                    indices.Add( current - 1 );
                    indices.Add( current + 1 );

                    indices.Add( current + 1 );
                    indices.Add( current + 2 );

                    indices.Add( current + 2 );
                    indices.Add( current );


                }
                else // EVENS
                {

                    int curHalf = ( (int)cell.ColRow.x ) / 2;
                    
                    int thisRowAdded = 3 + ( 3 *  curHalf ) + ( curHalf - 1) ;



                    int row = 3 + ( 3 * ( half ) ) + ( half ) + 1;
                    if ( odd == 0 ) row = 3 + ( 3 * ( half ) ) + ( half - 1 );

                    int current = row0 + ( ( (int)cell.ColRow.y - 1 ) * row ) + thisRowAdded;

                    int pastRow = 6 + ( 4 * ( (int)cell.ColRow.x - 1 ) );
                    if ( cell.ColRow.y > 1 ) pastRow = ( Mathf.Clamp( ( (int)cell.ColRow.y - 2 ), 0, (int)cell.ColRow.y ) * ( row ) ) + thisRowAdded + row0;


                    if (cell.ColRow.x == Grid.size - 1) // repl. col
                    {
                        indices.Add( current - 3 );
                        indices.Add( current + 1 );

                        indices.Add( current + 1);
                        indices.Add( current );

                        indices.Add( current );
                        if ( cell.ColRow.y > 1 ) indices.Add( pastRow + 1 );
                        else indices.Add( pastRow + 3 );

                    }
                    else
                    {
                        indices.Add( current - 3 );
                        indices.Add( current );

                        indices.Add( current );
                        if ( cell.ColRow.y > 1 ) indices.Add( pastRow + 2 );
                        else indices.Add( pastRow + 6 );
                    }

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


    private void OnDrawGizmos()
    {
        if ( Grid.GridVerts == null || Grid.GridVerts.Length < 0 ) return;
        for ( int j = 0; j < Grid.GridVerts.Length; j++ )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( Grid.GridVerts[j], 0.07f );

        }
    }

}
