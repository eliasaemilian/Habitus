using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Cell
{
    public Vector2 ColRow;
    public Vector3 WorldPos;
    public bool ElevatedOnZ;
    public Vector3[] Verts;

    public Tile Tile;
    public Vector2[] Neighbours;
    public bool[] IsConnected;

    public void GetNeighbours(int gridsize)
    {
      //  List<Vector2> n = new List<Vector2>();
        IsConnected = new bool[6];
        // get neighbour cells depending on own position

        if (ColRow.x == 0)
        {

            if ( ColRow.y != 0)
            {
                // 2, 3
                IsConnected[2] = true;
                IsConnected[3] = true;
            }

            if ( ColRow.y != gridsize - 1 )
            {
                // 0 , 1
                IsConnected[0] = true;
                IsConnected[1] = true;
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
            }
        }
        else
        {
            if ( ColRow.y != 0 )
            {
                // 3 , 2 , 1, 0
                IsConnected[0] = true;
                IsConnected[1] = true;
                IsConnected[2] = true;
                IsConnected[3] = true;

            }
            else if (ColRow.y != gridsize - 1)
            {
                // 0 , 1 , 5 , 4
                IsConnected[0] = true;
                IsConnected[1] = true;
                IsConnected[4] = true;
                IsConnected[5] = true;
            }
        }
    }

}



