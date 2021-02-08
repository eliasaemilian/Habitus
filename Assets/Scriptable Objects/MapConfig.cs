using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "MapConfig", menuName ="Configs/Map")]
public class MapConfig : ScriptableObject
{
    public TileType[,] TileTypes;
}
