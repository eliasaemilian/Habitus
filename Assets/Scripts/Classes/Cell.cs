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

    public Tile Tile;
    public Vector2Int[] Neighbours;
    public bool[] IsConnected;

    // HEX
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

    public Vector3[] GetVertsByNeighbours() //SIDES
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
        //if ( !IsConnected[2] ) verts.AddRange( new Vector3[3] { HVerts[HIndicesTop[0]], HVerts[HIndicesTop[1]], HVerts[HIndicesTop[2]] } );
        //if ( !IsConnected[3] ) verts.AddRange( new Vector3[3] { HVerts[HIndicesTop[3]], HVerts[HIndicesTop[4]], HVerts[HIndicesTop[5]] } );
        //if ( !IsConnected[4] ) verts.AddRange( new Vector3[3] { HVerts[HIndicesTop[6]], HVerts[HIndicesTop[7]], HVerts[HIndicesTop[8]] } );
        //if ( !IsConnected[5] ) verts.AddRange( new Vector3[3] { HVerts[HIndicesTop[9]], HVerts[HIndicesTop[10]], HVerts[HIndicesTop[11]] } );
        //if ( !IsConnected[0] ) verts.AddRange( new Vector3[3] { HVerts[HIndicesTop[12]], HVerts[HIndicesTop[13]], HVerts[HIndicesTop[14]] } );
        //if ( !IsConnected[1] ) verts.AddRange( new Vector3[3] { HVerts[HIndicesTop[15]], HVerts[HIndicesTop[16]], HVerts[HIndicesTop[17]] } );

        return verts.ToArray();
    }

}



