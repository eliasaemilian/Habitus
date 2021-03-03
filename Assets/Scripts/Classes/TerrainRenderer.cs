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
    public ComputeBuffer CmptBufferOut;
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
        if ( CmptBufferOut != null ) CmptBufferOut.Dispose();

        int countOut = GridVertices.Count * 5;

        CmptBuffer = new ComputeBuffer( GridVertices.Count, ( sizeof( float ) * 3 ) + sizeof ( int ), ComputeBufferType.Default );
        CmptBufferOut = new ComputeBuffer( countOut, ( sizeof( float ) * 3 ) + sizeof ( int ), ComputeBufferType.Default );
        CmptBuffer.SetData( GridVertices );

        int kernelHandle = Compute_Terrain.FindKernel( "CSMain" );
        Compute_Terrain.SetBuffer( kernelHandle, "HexVertices", CmptBuffer );
        Compute_Terrain.SetBuffer( kernelHandle, "Vertices", CmptBufferOut );

        Compute_Terrain.Dispatch( kernelHandle, 1, 1, 1 );

        Mat_Terrain.SetBuffer( "Vertices", CmptBufferOut );
        
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
