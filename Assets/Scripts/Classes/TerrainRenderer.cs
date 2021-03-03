using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

[Serializable]
public class TerrainRenderer
{
    public Material Mat_Terrain;
    public ComputeShader Compute_Terrain;
    public ComputeBuffer CmptBuffer;
    public int IndicesTotal { get {  return  Vertices.Count; } }

    // FOR DEBUG
    public List<Vector3> Vertices;

    public TerrainRenderer( Config_TerrainRenderer config ) 
    {
        Mat_Terrain = config.Mat_Terrain;
        Compute_Terrain = config.Compute_Terrain;

        Vertices = new List<Vector3>();

    }


    public void SetComputeBuffer()
    {
        if ( Vertices.Count <= 0 ) return;

        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        CmptBuffer = new ComputeBuffer( Vertices.Count, sizeof( float ) * 3, ComputeBufferType.Default );
        CmptBuffer.SetData( Vertices );
        Mat_Terrain.SetBuffer( "Vertices", CmptBuffer );
        
    }


    public void InitComputeShader()
    {

    }

    public void DispatchComputeShader()
    {

    }

    public void Cleanup()
    {
        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        Vertices.Clear();

    }
}
