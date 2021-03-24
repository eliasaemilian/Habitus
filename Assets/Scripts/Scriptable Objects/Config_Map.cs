using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to generated grid maps
/// </summary>
[CreateAssetMenu( fileName = "MapConfig", menuName ="Configs/Map")]
public class Config_Map : ScriptableObject
{
    // GRID INFO
    public int GridSize;
    public float TileSize;
    public float TileThickness;

    // TERRAIN ATTRIBUTES
    [Range(0, 1)] public float Mountyness;
    public List<Config_TerrainType> TerrainTypes;
}