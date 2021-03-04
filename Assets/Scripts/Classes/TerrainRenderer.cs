using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;

[Serializable]
public class TerrainRenderer
{
    public Material Mat_Terrain;
    public ComputeShader Compute_Terrain;
    public ComputeBuffer CmptBuffer;
    public ComputeBuffer CmptBufferOut;
    public ComputeBuffer CmptBufferHexagons;
    public int IndicesTotal { get {  return  GridVertices.Count; } }

    public List<GridVertex> GridVertices;
    public List<Hexagon> Hexagons;

    public GridVertex[] DebugOut;

    public MapConfig configMap;

    // FOR DEBUG
    // public List<Vector3> Vertices;

    public TerrainRenderer( Config_TerrainRenderer config, MapConfig conDebug ) 
    {
        Mat_Terrain = config.Mat_Terrain;
        Compute_Terrain = config.Compute_Terrain;

       // Vertices = new List<Vector3>();
        GridVertices = new List<GridVertex>();
        Hexagons = new List<Hexagon>();

        configMap = conDebug;
    }


    public void SetComputeBuffer()
    {
        if ( GridVertices.Count <= 0 ) return;

        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        if ( CmptBufferOut != null ) CmptBufferOut.Dispose();

        int countHex = Hexagons.Count;
        int countOutHex = Hexagons.Count * 6 * ( ( 2 * 4 ) + ( 2 * 2 ) );

        // tess 0 count = Hexagons.Count * 6 * 3;
        // tes 1 count = Hexagons.Count * 6 * ( ( 2 * 4 ) + ( 2 * 2 ) )

        CmptBuffer = new ComputeBuffer( GridVertices.Count, ( sizeof( float ) * 3 ) + sizeof ( int ), ComputeBufferType.Default );
        CmptBufferOut = new ComputeBuffer( countOutHex, ( sizeof( float ) * 3 ) + sizeof ( int ), ComputeBufferType.Default );
        CmptBufferHexagons = new ComputeBuffer( countHex, Marshal.SizeOf(typeof(Hexagon)), ComputeBufferType.Default );


        CmptBuffer.SetData( GridVertices );
        CmptBufferHexagons.SetData( Hexagons );

        int kernelHandle = Compute_Terrain.FindKernel( "CSMain" );

        Compute_Terrain.SetBuffer( kernelHandle, "HexVertices", CmptBuffer );

        Compute_Terrain.SetBuffer( kernelHandle, "Hex", CmptBufferHexagons );

        Compute_Terrain.SetBuffer( kernelHandle, "Vertices", CmptBufferOut );

        Compute_Terrain.SetFloat( "HexRadius", configMap.TileHeight * 0.5f );
        Compute_Terrain.SetFloat( "HexWidth", configMap.TileWidth * 0.5f );

        Compute_Terrain.Dispatch( kernelHandle, 1, 1, 1 );

        DebugOut = new GridVertex[countOutHex];
        CmptBufferOut.GetData( DebugOut );

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
