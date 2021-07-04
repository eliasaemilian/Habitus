using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "MapSave", menuName = "Saves/Map" )]
public class MapSave : ScriptableObject {

    public Config_Map mapConfig;
    public Setup_Render renderSetup;

    public Vector2[] gpuData; //  x > Terrain ID , y > Active 1, Inactive 0
    public int[] originalTerrain;
}