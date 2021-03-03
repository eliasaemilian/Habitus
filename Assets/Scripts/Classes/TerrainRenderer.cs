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
    public int IndicesTotal { get {  return  GridVertices.Count; } }

    public List<GridVertex> GridVertices;

    // FOR DEBUG
   // public List<Vector3> Vertices;

    public TerrainRenderer( Config_TerrainRenderer config ) 
    {
        Mat_Terrain = config.Mat_Terrain;
        Compute_Terrain = config.Compute_Terrain;

       // Vertices = new List<Vector3>();
        GridVertices = new List<GridVertex>();
    }


    public void SetComputeBuffer()
    {
        if ( GridVertices.Count <= 0 ) return;

        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        CmptBuffer = new ComputeBuffer( GridVertices.Count, ( sizeof( float ) * 3 ) + sizeof ( int ), ComputeBufferType.Default );
        CmptBuffer.SetData( GridVertices );
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
        GridVertices.Clear();

    }
}
