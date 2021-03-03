using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu( fileName = "TerrainRendererConfig", menuName = "Configs/Rendering/Terrain" )]
public class Config_TerrainRenderer : ScriptableObject
{
    public Material Mat_Terrain;
    public ComputeShader Compute_Terrain;

}
