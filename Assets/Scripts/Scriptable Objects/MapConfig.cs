using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to generated grid maps
/// </summary>
[CreateAssetMenu( fileName = "MapConfig", menuName ="Configs/Map")]
public class MapConfig : ScriptableObject
{
    // GRID INFO
    public int GridSize;
    public float TileHeight;
    public float TileWidth { get { return TileHeight * 0.8660254f ; } }

    public float TileThickness;

    // TERRAIN ATTRIBUTES
    [Range(0, 1)] public float Mountyness;
}
