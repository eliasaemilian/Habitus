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

    public void GetNeighbours(int gridsize)
    {
        List<Vector2> n = new List<Vector2>();
        // get neighbour cells depending on own position

        if (ColRow.x == 0)
        {

            if ( ColRow.y != 0)
            {
                // 2, 3
            }

            if ( ColRow.y != gridsize - 1 )
            {
                // 0 , 1
            }
        }
        else if ( ColRow.x == gridsize - 1)
        {

            if ( ColRow.y != 0 )
            {
                // 4 , 3
            }

            if ( ColRow.y != gridsize - 1)
            {
                // 5 , 0

            }
        }
        else
        {
            if ( ColRow.y != 0 )
            {
                // 3 , 2 , 1, 0
            }
            else if (ColRow.y != gridsize - 1)
            {
                // 0 , 1 , 5 , 4
            }
        }
    }

    public Vector2[] Neighbours;
}



