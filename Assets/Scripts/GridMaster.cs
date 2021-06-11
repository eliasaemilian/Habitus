using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMaster : InstantiatedGridComponent
{  
    float r; float w;




    private void Update()
    {
        if ( Input.GetMouseButtonUp( 0 ) ) OnClickNeighbourDebug();
      
    }
    private void ClearGrid()
    {
        if ( Grid.Cells == null || Grid.Cells.Length <= 0 ) return;


        Grid.CleanUp();
    }

    public void OnClickClearGrid() => ClearGrid();


    public bool GetCellOnClick(out Vector2Int hex)
    {
        hex = new Vector2Int();
        Plane plane = new Plane( Vector3.up, 0 );
        Ray ray = Camera.main.ScreenPointToRay( new Vector3( Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - Grid.TileThickness ) );
        if ( plane.Raycast( ray, out float pldist ) )
        {
            Vector3 p = ray.GetPoint( pldist );

            float col; float row;

            float unit = Grid.TileWidth + ( Grid.TileWidth * .5f );
            col = p.x / unit;
            col *= 2;

            // row
            unit = Grid.TileHeight;
            row = p.z / unit;

            int tC = Mathf.RoundToInt( col );
            int tR = Mathf.RoundToInt( row );

            Vector2 pos = new Vector2( p.x, p.z );

            // Search for the nearest hexagon
            float minimum = 2 * Grid.TileWidth;  
            for ( int x = tC - 1; x <= tC + 1; ++x )
                for ( int y = tR - 1; y <= tR + 1; ++y )
                {
                    if ( x < 0 || x >= Grid.Size || y < 0 || y >= Grid.Size ) continue;
                    float dist = Vector2.Distance( new Vector2( Grid.Cells[x, y].WorldPos.x, Grid.Cells[x, y].WorldPos.z ), pos );
                    if ( dist < minimum )
                    {
                        minimum = dist;
                        hex.x = x;
                        hex.y = y;
                    }
                }
            return true;
        }
        return false;
    }


    // ------------------------------------------------------------- DEBUG FUNCTIONS ------------------------------------------------------------ //

    public void OnClickNeighbourDebug()
    {
        Debug.Log( "Click" );
        if ( GetCellOnClick( out Vector2Int hex ) )
        {

          //  Grid.Cells[hex.x, hex.y].Tile.RefGO.GetComponent<MeshRenderer>().materials[1].color = Color.red;
          //  Grid.Cells[hex.x, hex.y].SetNeighbours( Grid.Size );
            Debug.Log( $"Clicked on Col: {Grid.Cells[hex.x, hex.y].ColRow.x}, Row: {Grid.Cells[hex.x, hex.y].ColRow.y}." );
            uint id = Grid.GetHexIDByColRow( hex.y, hex.x );
           
            Grid.RemoveHexagon( new uint[] { id } );
            Debug.Log( "Removed " + id );
        }

    }

    #region CPU ALT TODO
    // ---------------------------------------- CPU ALT CODE ---------------------------------------- //
    // TODO: Implement as alt if Computeshaders are not supported by gpu
    private void GenGridVertices()
    {
        if ( Grid.GridPoints.Length <= 0 ) return;

        // vertices
        List<int> indices = new List<int>();
        List<Vector3> verts = new List<Vector3>();

        r = Grid.TileHeight * .5f;
        w = Grid.TileWidth * .5f;

        for ( int i = 0; i < Grid.CenterPoints.Length; i++ )
        {
            Vector3 c = new Vector3( Grid.CenterPoints[i].x, 0, Grid.CenterPoints[i].z ); // center of tile
            float y = Grid.TileThickness;

            Vector3 p1 = new Vector3( c.x + r, y, c.z ); // 0
            Vector3 p2 = new Vector3( c.x + r * .5f, y, c.z - w ); // 1
            Vector3 p3 = new Vector3( c.x - r * .5f, y, c.z - w ); // 2
            Vector3 p4 = new Vector3( c.x - r, y, c.z ); // 3
            Vector3 p5 = new Vector3( c.x - r * .5f, y, c.z + w ); // 4
            Vector3 p6 = new Vector3( c.x + r * .5f, y, c.z + w ); // 5

            Cell cell = Grid.CellsQueued[i];
            //cell.Verts = GenHexagonMeshInfo( cell, c, new Vector3[6] { p1, p2, p3, p4, p5, p6 } );

            List<Vector3> cverts = new List<Vector3>();
            // vertices
            if ( cell.ColRow.y == 0 ) // ROW 0
            {
                if ( cell.ColRow.x == 0 )
                {
                    cverts.Add( p1 );
                    cverts.Add( p2 );
                    cverts.Add( p3 );
                    cverts.Add( p4 );
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
                        if ( cell.ColRow.x == Grid.Size - 1 ) cverts.Add( p1 ); //repl. col
                    }

                }

                cverts.Add( p6 );

            }

            verts.AddRange( cverts );


        }

        for ( int i = 0; i < Grid.CenterPoints.Length; i++ )
        {
            indices.AddRange( CalcIndicesGridPoints( Grid.CellsQueued[i] ) );
        }


        Grid.RasterVertices = new Vector3[indices.Count];

        for ( int j = 0; j < Grid.RasterVertices.Length; j++ )
        {
            Grid.RasterVertices[j] = verts[indices[j]];
        }

    }
   
    private List<int> CalcIndicesGridPoints( Cell cell )
    {
        List<int> indices = new List<int>();
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
            int row0 = 6 + ( ( Grid.Size - 1 ) * 4 ); // repl col for all 3
            int half = ( Grid.Size / 2 );
            int odd = ( Grid.Size % 2 != 0 ) ? 1 : 0;

            if ( cell.ColRow.x == 0 )
            {

                int row = 3 + ( 3 * ( half ) ) + ( half ) + 1;
                if ( odd == 0 ) row = 3 + ( 3 * ( half ) ) + ( half - 1 );

                int current = row0 + ( ( (int)cell.ColRow.y - 1 ) * row ); // first index newly added in this row


                int pastRow = 6 + ( ( (int)cell.ColRow.x ) * 4 ) - 1;
                if ( cell.ColRow.y == 1 ) pastRow = 0;

                if ( cell.ColRow.y > 1 ) pastRow = row0 + ( ( ( (int)cell.ColRow.y - 2 ) * row ) );

                if ( cell.ColRow.y != 1 )
                {
                    indices.Add( pastRow + 1 );
                    indices.Add( current );

                    indices.Add( current );
                    indices.Add( current + 1 );

                    indices.Add( current + 1 );
                    indices.Add( current + 2 );

                    indices.Add( current + 2 );
                    indices.Add( pastRow + 4 );
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

                int curHalf = ( (int)cell.ColRow.x - 1 ) / 2;

                if ( curHalf < 0 ) curHalf = 0;

                int thisRowAdded = 3 + ( 3 * curHalf ) + ( curHalf );
                if ( cell.ColRow.x > 1 ) thisRowAdded = ( 3 * curHalf ) + ( curHalf + 1 ) + 1; // 0 counts as even, add +1 since its 3 not 2 verts added


                int row = 3 + ( 3 * ( half ) ) + ( half ) + 1;
                if ( odd == 0 ) row = 3 + ( 3 * ( half ) ) + ( half - 1 );

                int current = row0 + ( ( (int)cell.ColRow.y - 1 ) * row ) + thisRowAdded; // first index newly added in this row + everything added to this row - > + 1 this is where we are now
                if ( cell.ColRow.x > 1 ) current += 1;

                int pastRow = 6 + ( 4 * ( (int)cell.ColRow.x - 1 ) );
                if ( cell.ColRow.y > 1 ) pastRow = ( Mathf.Clamp( ( (int)cell.ColRow.y - 2 ), 0, (int)cell.ColRow.y ) * ( row ) ) + thisRowAdded + row0;
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

                int thisRowAdded = 3 + ( 3 * curHalf ) + ( curHalf - 1 );



                int row = 3 + ( 3 * ( half ) ) + ( half ) + 1;
                if ( odd == 0 ) row = 3 + ( 3 * ( half ) ) + ( half - 1 );

                int current = row0 + ( ( (int)cell.ColRow.y - 1 ) * row ) + thisRowAdded;

                int pastRow = 6 + ( 4 * ( (int)cell.ColRow.x - 1 ) );
                if ( cell.ColRow.y > 1 ) pastRow = ( Mathf.Clamp( ( (int)cell.ColRow.y - 2 ), 0, (int)cell.ColRow.y ) * ( row ) ) + thisRowAdded + row0;


                if ( cell.ColRow.x == Grid.Size - 1 ) // repl. col
                {
                    indices.Add( current - 3 );
                    indices.Add( current + 1 );

                    indices.Add( current + 1 );
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
        return indices;
    }

    // ---- END CPU ALT CODE
    #endregion
}
