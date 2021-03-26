using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to generated grid maps
/// </summary>
[CreateAssetMenu( fileName = "MapConfig", menuName ="Configs/Map")]
public class Config_Map : ScriptableObject
{
    [Header( "Grid Info" )]
    public int GridSize;
    public float TileSize;
    public float TileThickness;

    [Header ( "Terrain Attributes" )]
    [Range(0, 1)] public float Mountyness;
    public List<Config_Terrain> Terrains;
}