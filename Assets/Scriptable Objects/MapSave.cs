using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "MapSave", menuName = "Saves/Map" )]
public class MapSave : ScriptableObject
{
    public TileType[,] TileTypes;
}