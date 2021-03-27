using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;


[Serializable]
public class TerrainRenderer
{

    private TerrainType _terrainType;
    public TerrainType TerrainType { get { return _terrainType; } }

    public int GetID {  get { return TerrainType.ID; } }

    const int NUM_THREADS = 8;

    public Material Mat_Terrain;
    public ComputeShader Compute_Terrain;
    public ComputeBuffer CmptBuffer;
    public ComputeBuffer CmptBufferOut;
    public ComputeBuffer CmptBufferHexagons;
    public ComputeBuffer CmptBufferHexagons2;
  //  public int IndicesTotal { get {  return  GridVertices.Count; } }

   // public List<GridVertex> GridVertices;

    public Hexagon[,] Hexagons;
    public int HexagonCount { get { return Hexagons.Length; } }
    public Hexagon.GPU[,] HexBuffer;

    public GridVertex[] DebugOut;
    public Hexagon[] DebugOutHex;


    public int count;


    private Vector4 size;


    public TerrainRenderer( int gridSize, float tileSize, Config_Terrain config ) 
    {
        _terrainType = new TerrainType( config );
        Mat_Terrain = config.Mat_Terrain;
        Compute_Terrain = config.Compute_Terrain;
        
        Hexagons = new Hexagon[gridSize, gridSize];
        HexBuffer = new Hexagon.GPU[gridSize, gridSize];

        size = new Vector4( gridSize, gridSize, tileSize * 0.5f, tileSize * 0.5f );

    }




    public void SetComputeBuffer()
    {
      //  if ( GridVertices.Count <= 0 ) return;

        if ( CmptBuffer != null ) CmptBuffer.Dispose();
        if ( CmptBufferOut != null ) CmptBufferOut.Dispose();


        // get count by tesselation
        Vector4 tesselation = new Vector4( 0, 0, 1, 0 );



        count = GetVertexCount();

        if ( count <= 0 ) return;

        CmptBufferOut = new ComputeBuffer( count, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );
        CmptBufferHexagons = new ComputeBuffer( (int)(size.x * size.y), Marshal.SizeOf(typeof(Hexagon.GPU)), ComputeBufferType.Default );
        ComputeBuffer activeVertsOut = new ComputeBuffer( count, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );

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
                if ( tesselation.x == 1 ) count += 6 * 3;
                else if ( tesselation.y == 1 ) count += 3 * ( ( 2 * 4 ) + ( 2 * 2 ) );
                else if ( tesselation.z == 1 ) count += 3 * ( ( 2 * 12 ) + ( 2 * 4 ) );
                
            }
        }

        Debug.Log( "Vertex Count for Terrain " + _terrainType.ID + " was " + count );
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
        h.Type = TerrainType;

        Hexagons[x, y] = h;
        HexBuffer[x, y] = h.gpu;
    }

    public void CleanUp()
    {
        if ( CmptBuffer != null ) CmptBuffer.Dispose();
    }
}
