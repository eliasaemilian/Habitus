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

    private Material _matTerrain;
    public Material MatTerrain { get { return _matTerrain; } }

    private ComputeShader _computeTerrainShader;
    public ComputeShader ComputeTerrainShader { get { return _computeTerrainShader; } }
    public int HexagonCount { get { return _hexagons.Length; } }
    public int VerticesCount { get { return GetVertexCount(); } }

    private Hexagon[,] _hexagons;
    private Hexagon.GPU[,] _hexBuffer;

    private ComputeBuffer _cmptBufferOut;
    private ComputeBuffer _cmptBufferHexagons;
    private Vector4 _size;


    public Hexagon.GPU[] DEBUGHEX;

    public TerrainRenderer( int gridSize, float tileHeight, float tileWidth, Config_Terrain config ) 
    {
        _matTerrain = config.Mat_Terrain;
        _computeTerrainShader = WorkaroundInstantiateCmptShader.InstantiateComputeShader( config.Compute_Terrain ); //( Resources.Load<ComputeShader>( "ComputeShader/Cmpt_GridVertices" ) ); 

        _hexagons = new Hexagon[gridSize, gridSize];
        _hexBuffer = new Hexagon.GPU[gridSize, gridSize];

        _size = new Vector4( gridSize, gridSize, tileHeight * 0.5f, tileWidth * 0.5f );

    }


    public void SetComputeBuffer()
    {
        CleanUp();

        if ( VerticesCount <= 0 ) return;

        _cmptBufferOut = new ComputeBuffer( VerticesCount, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );
        _cmptBufferHexagons = new ComputeBuffer( (int)(_size.x * _size.y), Marshal.SizeOf(typeof(Hexagon.GPU)), ComputeBufferType.Default );
        ComputeBuffer activeVertsOut = new ComputeBuffer( VerticesCount, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );

        _cmptBufferHexagons.SetData( _hexBuffer );

        int kernelHandle = ComputeTerrainShader.FindKernel( "GenGridVertices" );
        int kernelHandleBorder = ComputeTerrainShader.FindKernel( "GenMeshBorder" );

        ComputeTerrainShader.SetBuffer( kernelHandle, "HexInput", _cmptBufferHexagons );
        ComputeTerrainShader.SetBuffer( kernelHandle, "Vertices", _cmptBufferOut );
        ComputeTerrainShader.SetBuffer( kernelHandle, "ActiveVerticesOut", activeVertsOut );

        ComputeTerrainShader.SetVector( "_Size", _size );

        ComputeTerrainShader.Dispatch( kernelHandle, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, 1 );

        // BORDER MESH
        ComputeTerrainShader.SetBuffer( kernelHandleBorder, "HexInput", _cmptBufferHexagons );
        ComputeTerrainShader.Dispatch( kernelHandleBorder, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, 1 );


        // 
        Hexagon.GPU[,] debugOut = new Hexagon.GPU[(int)_size.x, (int)_size.y];
        _cmptBufferHexagons.GetData( debugOut );
        List<Hexagon.GPU> hex = new List<Hexagon.GPU>();
        for ( int i = 0; i < debugOut.GetLength(0); i++ )
        {
            for ( int j = 0; j < debugOut.GetLength( 1 ); j++ )
            {
                hex.Add( _hexBuffer[i, j] );
                Debug.Log( "HEX RESULT " + debugOut[i, j].topvert0 );
            }
        }
        DEBUGHEX = hex.ToArray();

        MatTerrain.SetBuffer( "Vertices", _cmptBufferOut );

    }

    private int GetVertexCount()
    {
        int count = 0;

        for ( int i = 0; i < _hexBuffer.GetLength( 0 ); i++ )
        {
            for ( int j = 0; j < _hexBuffer.GetLength( 1 ); j++ )
            {
                Vector4 tesselation = _hexBuffer[i, j].tesselation;
                if ( tesselation.x == 1 ) count += 6 * 3;
                else if ( tesselation.y == 1 ) count += 3 * ( ( 2 * 4 ) + ( 2 * 2 ) );
                else if ( tesselation.z == 1 ) count += 3 * ( ( 2 * 12 ) + ( 2 * 4 ) );
                // else if ( tesselation.w == 1 ) MATHS GO HERE 
            }
        }

        return count;
    }



    public void AddHexagonToRenderer(int x, int y, Hexagon h)
    {
        _hexagons[x, y] = h;
        _hexBuffer[x, y] = h.gpu;
    }

    public void CleanUp()
    {
        if ( _cmptBufferOut != null ) _cmptBufferOut.Dispose();
        if ( _cmptBufferHexagons != null ) _cmptBufferHexagons.Dispose();
    }
}
