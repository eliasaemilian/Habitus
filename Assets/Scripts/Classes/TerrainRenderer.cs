using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;

[Serializable]
public class TerrainRenderer
{
    const int NUM_THREADS = 8;

    public Material Mat_Terrain;
    public ComputeShader Compute_Terrain;
    public ComputeBuffer CmptBuffer;
    public ComputeBuffer CmptBufferOut;
    public ComputeBuffer CmptBufferHexagons;
    public ComputeBuffer CmptBufferHexagons2;
    public int IndicesTotal { get {  return  GridVertices.Count; } }

    public List<GridVertex> GridVertices;
    public List<Hexagon> Hexagons;

    public Hexagon[,] HexBuffer;

    public GridVertex[] DebugOut;
    public Hexagon[] DebugOutHex;

    public MapConfig configMap;

    public int count;

    // FOR DEBUG
    // public List<Vector3> Vertices;

    public TerrainRenderer( Config_TerrainRenderer config, MapConfig conDebug ) 
    {
        Mat_Terrain = config.Mat_Terrain;
        Compute_Terrain = config.Compute_Terrain;

       // Vertices = new List<Vector3>();
        GridVertices = new List<GridVertex>();
        Hexagons = new List<Hexagon>();
        HexBuffer = new Hexagon[conDebug.GridSize,conDebug.GridSize];
        configMap = conDebug;

    }


    public void SetComputeBuffer()
    {
        if ( GridVertices.Count <= 0 ) return;

        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        if ( CmptBufferOut != null ) CmptBufferOut.Dispose();

        int countHex = Hexagons.Count;
        int countOutHex = 1;



        // get count by tesselation
        Vector4 tesselation = new Vector4( 1, 0, 0, 0 );
        Vector4 size = new Vector4( configMap.GridSize, configMap.GridSize, 0, 0 );

        if ( tesselation.x == 1 ) countOutHex = Hexagons.Count * 6 * 3;
        else if (tesselation.y == 1) countOutHex = Hexagons.Count * 3 * ( ( 2 * 4 ) + ( 2 * 2 ) );
        else if (tesselation.z == 1) countOutHex = Hexagons.Count * 3 * ( ( 2 * 12 ) + ( 2 * 4 ) );
        count = countOutHex;

        CmptBuffer = new ComputeBuffer( GridVertices.Count, Marshal.SizeOf(typeof(GridVertex)), ComputeBufferType.Default );
        CmptBufferOut = new ComputeBuffer( countOutHex, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );
        CmptBufferHexagons = new ComputeBuffer( countHex, Marshal.SizeOf(typeof(Hexagon)), ComputeBufferType.Default );
        CmptBufferHexagons2 = new ComputeBuffer( (int)(size.x * size.y), Marshal.SizeOf(typeof(Hexagon)), ComputeBufferType.Default );
        ComputeBuffer debugOut = new ComputeBuffer( countHex, Marshal.SizeOf( typeof( Hexagon ) ), ComputeBufferType.Default );

        CmptBuffer.SetData( GridVertices );
        CmptBufferHexagons.SetData( Hexagons );
        CmptBufferHexagons2.SetData( HexBuffer );

        int kernelHandle = Compute_Terrain.FindKernel( "CSMain" );

        Compute_Terrain.SetBuffer( kernelHandle, "HexVertices", CmptBuffer );

        Compute_Terrain.SetBuffer( kernelHandle, "Hex", CmptBufferHexagons );
        Compute_Terrain.SetBuffer( kernelHandle, "Hex2", CmptBufferHexagons2 );

        Compute_Terrain.SetBuffer( kernelHandle, "Vertices", CmptBufferOut );
        Compute_Terrain.SetBuffer( kernelHandle, "debugHexOut", debugOut );

        Compute_Terrain.SetFloat( "HexRadius", configMap.TileHeight * 0.5f );
        Compute_Terrain.SetFloat( "HexWidth", configMap.TileWidth * 0.5f );
        Compute_Terrain.SetVector( "_Size", size );
        Compute_Terrain.SetVector( "Tesselation", tesselation );

        Compute_Terrain.Dispatch( kernelHandle, (int)size.x / NUM_THREADS, (int)size.y / NUM_THREADS, 1 );

        DebugOut = new GridVertex[countOutHex];
        DebugOutHex = new Hexagon[countHex];
        CmptBufferOut.GetData( DebugOut );
        debugOut.GetData( DebugOutHex );

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
