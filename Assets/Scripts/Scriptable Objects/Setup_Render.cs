using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "Setup_Render", menuName = "Configs/Setups" )]
public class Setup_Render : Setup
{
    [Header( "Terrain Renderer" )]
    public ComputeShader Compute_Grid;
    public Material Mat_Terrain;
    public Material Mat_Border;
    public Material Mat_Raster;
}

public abstract class Setup : ScriptableObject { }
