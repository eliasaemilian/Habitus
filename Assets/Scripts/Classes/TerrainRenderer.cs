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
    // public List<Hexagon> Hexagons;

    public Hexagon[,] Hexagons;
   public int HexagonCount { get { return Hexagons.Length; } }
    public Hexagon.GPU[,] HexBuffer;

    public GridVertex[] DebugOut;
    public Hexagon[] DebugOutHex;

    public Config_Map configMap;

    public int count;

    // FOR DEBUG

    

    public TerrainRenderer( Config_TerrainRenderer config, Config_Map conDebug ) 
    {
        Mat_Terrain = config.Mat_Terrain;
        Compute_Terrain = config.Compute_Terrain;

       // Vertices = new List<Vector3>();
        GridVertices = new List<GridVertex>();
        //Hexagons = new List<HexaOld>();
        Hexagons = new Hexagon[conDebug.GridSize,conDebug.GridSize];
        HexBuffer = new Hexagon.GPU[conDebug.GridSize,conDebug.GridSize];
        configMap = conDebug;


        
    }



    public void SetComputeBuffer()
    {
        if ( GridVertices.Count <= 0 ) return;

        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        if ( CmptBufferOut != null ) CmptBufferOut.Dispose();

        int countOutHex = 1;



        // get count by tesselation
        Vector4 tesselation = new Vector4( 0, 0, 1, 0 );
        Vector4 size = new Vector4( configMap.GridSize, configMap.GridSize, configMap.TileSize * 0.5f, configMap.TileWidth * 0.5f );

        //if ( tesselation.x == 1 ) countOutHex = Hexagons.Count * 6 * 3;
        //else if (tesselation.y == 1) countOutHex = Hexagons.Count * 3 * ( ( 2 * 4 ) + ( 2 * 2 ) );
        //else if (tesselation.z == 1) countOutHex = Hexagons.Count * 3 * ( ( 2 * 12 ) + ( 2 * 4 ) );
        //count = countOutHex;

        count = GetVertexCount(); // TODO: Fill Terrain Types for Hexagons

        CmptBufferOut = new ComputeBuffer( count, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );
        CmptBufferHexagons = new ComputeBuffer( (int)(size.x * size.y), Marshal.SizeOf(typeof(Hexagon.GPU)), ComputeBufferType.Default );
        ComputeBuffer activeVertsOut = new ComputeBuffer( countOutHex, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );

        CmptBufferHexagons.SetData( HexBuffer );

        int kernelHandle = Compute_Terrain.FindKernel( "GenGridVertices" );

        Compute_Terrain.SetBuffer( kernelHandle, "HexInput", CmptBufferHexagons );
        Compute_Terrain.SetBuffer( kernelHandle, "Vertices", CmptBufferOut );
        Compute_Terrain.SetBuffer( kernelHandle, "ActiveVerticesOut", activeVertsOut );

        Compute_Terrain.SetVector( "_Size", size );
        Compute_Terrain.SetVector( "_Tesselation", tesselation );

        Compute_Terrain.Dispatch( kernelHandle, ( (int)size.x * (int)size.y ) / NUM_THREADS, ( (int)size.x * (int)size.y ) / NUM_THREADS, 1 );

        DebugOut = new GridVertex[count];
        CmptBufferOut.GetData( DebugOut );

        Mat_Terrain.SetBuffer( "Vertices", CmptBufferOut );

        // DEBUG
       // FillDebug();
    }

    private int GetVertexCount()
    {
        int count = 0;

        for ( int i = 0; i < HexBuffer.GetLength( 0 ); i++ )
        {
            for ( int j = 0; j < HexBuffer.GetLength( 1 ); j++ )
            {
                Vector4 tesselation = ( HexBuffer[i, j].tesselation );
                if ( tesselation.x == 1 ) count += HexagonCount * 6 * 3;
                else if ( tesselation.y == 1 ) count += HexagonCount * 3 * ( ( 2 * 4 ) + ( 2 * 2 ) );
                else if ( tesselation.z == 1 ) count += HexagonCount * 3 * ( ( 2 * 12 ) + ( 2 * 4 ) );
            }
        }

        return count;
    }



    public void InitComputeShader()
    {

    }

    public void DispatchComputeShader()
    {

    }


    public void AddHexagonToRenderer(int x, int y, Hexagon h)
    {
        Hexagons[x, y] = h;
        HexBuffer[x, y] = h.gpu;
    }

    public void Cleanup()
    {
        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        GridVertices.Clear();

    }
}
