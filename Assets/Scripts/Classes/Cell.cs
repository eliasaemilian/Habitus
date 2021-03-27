using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Cell
{
    public Vector2Int ColRow;
    public Vector3 WorldPos;
    public bool ElevatedOnZ;
    public Vector3[] Verts;

    public Hexagon Hexagon;
    public Vector2Int[] Neighbours;
    public bool[] IsConnected;

    // HEX
    public Vector3 Center;

    public Vector3[] HVerts;
    public int[] HIndicesTop;
    public int[] HIndicesSides;

    public void SetNeighbours(int gridsize)
    {
      //  List<Vector2> n = new List<Vector2>();
        IsConnected = new bool[6];
        // get neighbour cells depending on own position
        //Debug.Log( $"Searching Neighbours for Col: {ColRow.x}, Row: {ColRow.y}" );
        if (ColRow.x == 0)
        {

            if ( ColRow.y != 0)
            {
                // 2, 3
                IsConnected[2] = true;
                IsConnected[3] = true;
                //Debug.Log( "2, 3" );
            }

            if ( ColRow.y != gridsize - 1 )
            {
                // 0 , 1
                IsConnected[0] = true;
                IsConnected[1] = true;
                if ( ColRow.x % 2 != 0 ) IsConnected[2] = true;
            }
        }
        else if ( ColRow.x == gridsize - 1)
        {

            if ( ColRow.y != 0 )
            {
                // 4 , 3
                IsConnected[4] = true;
                IsConnected[3] = true;
            }

            if ( ColRow.y != gridsize - 1)
            {
                // 5 , 0
                IsConnected[5] = true;
                IsConnected[0] = true;
                if ( ColRow.x % 2 != 0 ) IsConnected[4] = true;
            }
        }
        else
        {
            if ( ColRow.y != 0 && ColRow.y != gridsize - 1 )
            {
                // 3 , 2 , 1, 0
                IsConnected[0] = true;
                IsConnected[1] = true;
                IsConnected[2] = true;
                IsConnected[3] = true;
                IsConnected[4] = true;
                IsConnected[5] = true;

            }
            else if ( ColRow.y == 0 )
            {
                // 0 , 1 , 5
                IsConnected[0] = true;
                IsConnected[1] = true;
                IsConnected[5] = true;

                if ( ColRow.x % 2 != 0) // odd
                {
                    // 0 , 1 , 2, 5 , 4
                    IsConnected[2] = true;
                    IsConnected[4] = true;
                }

            }
            else if ( ColRow.y == gridsize - 1)
            {

                IsConnected[3] = true;
                IsConnected[2] = true;
                IsConnected[4] = true;

                if ( ColRow.x % 2 == 0 ) // even
                {
                    IsConnected[1] = true;
                    IsConnected[5] = true;
                }
            }
        }
    }

    public Vector3[] GetBorderVerticesByNeighbour() 
    {
        List<Vector3> verts = new List<Vector3>();        
        if ( !IsConnected[2] ) verts.AddRange( new Vector3[6] { HVerts[HIndicesSides[0]], HVerts[HIndicesSides[1]], HVerts[HIndicesSides[2]], HVerts[HIndicesSides[3]], HVerts[HIndicesSides[4]], HVerts[HIndicesSides[5]] } );
        if ( !IsConnected[3] ) verts.AddRange( new Vector3[6] { HVerts[HIndicesSides[6]], HVerts[HIndicesSides[7]], HVerts[HIndicesSides[8]], HVerts[HIndicesSides[9]], HVerts[HIndicesSides[10]], HVerts[HIndicesSides[11]] } );
        if ( !IsConnected[4] ) verts.AddRange( new Vector3[6] { HVerts[HIndicesSides[12]], HVerts[HIndicesSides[13]], HVerts[HIndicesSides[14]], HVerts[HIndicesSides[15]], HVerts[HIndicesSides[16]], HVerts[HIndicesSides[17]] } );
        if ( !IsConnected[5] ) verts.AddRange( new Vector3[6] { HVerts[HIndicesSides[18]], HVerts[HIndicesSides[19]], HVerts[HIndicesSides[20]], HVerts[HIndicesSides[21]], HVerts[HIndicesSides[22]], HVerts[HIndicesSides[23]] } );
        if ( !IsConnected[0] ) verts.AddRange( new Vector3[6] { HVerts[HIndicesSides[24]], HVerts[HIndicesSides[25]], HVerts[HIndicesSides[26]], HVerts[HIndicesSides[27]], HVerts[HIndicesSides[28]], HVerts[HIndicesSides[29]] } );
        if ( !IsConnected[1] ) verts.AddRange( new Vector3[6] { HVerts[HIndicesSides[30]], HVerts[HIndicesSides[31]], HVerts[HIndicesSides[32]], HVerts[HIndicesSides[33]], HVerts[HIndicesSides[34]], HVerts[HIndicesSides[35]] } );
        return verts.ToArray();
    }

    public Vector3[] GetTopVerts()
    {
        List<Vector3> verts = new List<Vector3>();
        for ( int i = 0; i < HIndicesTop.Length; i++ )
        {
            verts.Add( HVerts[HIndicesTop[i]] );
        }

        return verts.ToArray();
    }

    private void InitSideVertices()
    {
        Vector3[] verts = new Vector3[24 + 2];
        int[] indices = new int[( 3 * 24 ) + 36];


        int c = 0;

        // sides

        // vertices
        //for ( int i = 0; i < oVerts.Length; i++ )
        //{
        //    verts[c] = new Vector3( oVerts[i].x, 0, oVerts[i].z );
        //    verts[c + 1] = new Vector3( oVerts[i].x, 0, oVerts[i].z );
        //    c += 2;
        //}
        //c = 14;
        //for ( int i = 0; i < oVerts.Length; i++ )
        //{
        //    verts[c] = oVerts[i];
        //    verts[c + 1] = oVerts[i];
        //    c += 2;
        //}


        // indices
        for ( int i = 0; i < 5; i++ )
        {
            indices[c] = 15 + ( i * 2 );
            indices[c + 1] = ( i * 2 ) + 2;
            indices[c + 2] = 15 + ( i * 2 ) + 2;

            indices[c + 3] = ( i * 2 ) + 2;
            indices[c + 4] = ( i * 2 ) + 4;
            indices[c + 5] = 15 + ( i * 2 ) + 2;

            c += 6;
        }

        indices[c] = 12;
        indices[c + 1] = 15;
        indices[c + 2] = 25;

        indices[c + 3] = 15;
        indices[c + 4] = 12;
        indices[c + 5] = 2;


    }

    private Vector3[] GenHexagonMeshInfo( Cell cell, Vector3 center, Vector3[] oVerts ) // TODO: REFACTOR
    {
        // verts
        Vector3[] verts = new Vector3[24 + 2];
        int[] otherIndices = new int[( 3 * 24 ) + 36];
        int[] topPlane = new int[3 * 12];
        int c = 1;
      //  Vector3 cTop = new Vector3( center.x, center.y + Grid.TileThickness, center.z ); // index 13
        Vector3 cBot = center; // index 0
        verts[0] = cBot;
 //       verts[13] = cTop;

   //     cell.Center = cTop;


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

        // sides
        for ( int i = 0; i < 5; i++ )
        {
            otherIndices[c] = 15 + ( i * 2 );
            otherIndices[c + 1] = ( i * 2 ) + 2;
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


        c += 6;
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


        cell.HVerts = verts;
        cell.HIndicesTop = topPlane;
        cell.HIndicesSides = otherIndices;

        return verts;
    }


}



