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
}



