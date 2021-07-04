using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to generated grid maps
/// </summary>
[CreateAssetMenu( fileName = "MapConfig", menuName = "Configs/Map" )]
public class Config_Map : ScriptableObject {
    public string MapName;

    [Header( "Grid Info" )]
    public int GridSize;
    public float TileSize;
    public float TileThickness;

    [Header( "Terrain Attributes" )]
    [Range( 0, 1 )] public float Mountyness;

    [Header( "Terrains" )]
    public Config_Terrain BlankTerrain;
    public Config_Terrain MountainTerrain;

    public Config_Terrain GetTerrainConfigByID( int id ) {
        if ( id == BlankTerrain.ID ) return BlankTerrain;
        else if ( id == MountainTerrain.ID ) return MountainTerrain;
        else return BlankTerrain;
    }
}