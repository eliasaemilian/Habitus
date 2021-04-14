using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;


[Serializable]
public class TerrainRenderer
{
    public List<uint> ActiveHexagons = new List<uint>(); // DEBUG: LATER SET BUFFER DIRECTLY
    const int NUM_THREADS = 8;

    private Material _matTerrain;
    public Material MatTerrain { get { return _matTerrain; } }

    private Material _matBorder;
    public Material Mat_Border { get { return _matBorder; } }

    private ComputeShader _computeTerrainShader;
    public ComputeShader ComputeTerrainShader { get { return _computeTerrainShader; } }
    public int HexagonCount { get { return _hexagons.Length; } }
    public int VerticesCount { get { return GetVertexCount(); } }
    public int VCountBorder { get { return GetBorderVCount(); } }


    private Hexagon[,] _hexagons;
    private Hexagon.GPU[,] _hexBuffer;

    private ComputeBuffer _cmptBufferOut;
    private ComputeBuffer _cmptBufferHexagons;
    private ComputeBuffer _cmptBorderOut;
    private ComputeBuffer _cmptActiveBorderIDs;
    private ComputeBuffer _cmptActiveBorderVertices;

    private Vector4 _size;


    public Hexagon.GPU[] DEBUGHEX;

    public TerrainRenderer( int gridSize, float tileHeight, float tileWidth, Setup_Render setup ) 
    {
        _matTerrain = setup.Mat_Terrain;
        _computeTerrainShader = setup.Compute_Grid; //( Resources.Load<ComputeShader>( "ComputeShader/Cmpt_GridVertices" ) ); 

        _matBorder = setup.Mat_Border;

        _hexagons = new Hexagon[gridSize, gridSize];
        _hexBuffer = new Hexagon.GPU[gridSize, gridSize];

        _size = new Vector4( gridSize, gridSize, tileHeight * 0.5f, tileWidth * 0.5f );

    }


    public void SetComputeBuffer()
    {
        CleanUp();

        if ( ActiveHexagons.Count <= 0 ) return;
        if ( VerticesCount <= 0 ) return;

        _cmptBufferOut = new ComputeBuffer( VerticesCount, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );
        _cmptBufferHexagons = new ComputeBuffer( (int)(_size.x * _size.y), Marshal.SizeOf(typeof(Hexagon.GPU)), ComputeBufferType.Default );
        _cmptBorderOut = new ComputeBuffer( VCountBorder, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Default );
        _cmptBufferHexagons.SetData( _hexBuffer );

        int kernelHandle = ComputeTerrainShader.FindKernel( "GenGridVertices" );
        int kernelHandleBorder = ComputeTerrainShader.FindKernel( "GenMeshBorder" );

        ComputeTerrainShader.SetBuffer( kernelHandle, "HexInput", _cmptBufferHexagons );

        ComputeTerrainShader.SetBuffer( kernelHandle, "Vertices", _cmptBufferOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBorder, "BorderVertices", _cmptBorderOut );

        ComputeTerrainShader.SetVector( "_Size", _size );

        ComputeTerrainShader.Dispatch( kernelHandle, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, 1 );

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
               // Debug.Log( "HEX RESULT " + debugOut[i, j].topvert0 );
            }
        }
        DEBUGHEX = hex.ToArray();



        // BORDER
        int bordercount = GetActiveBorderVCount();
        _cmptActiveBorderIDs = new ComputeBuffer( ActiveHexagons.Count, sizeof( uint ), ComputeBufferType.Append );
        _cmptActiveBorderVertices = new ComputeBuffer( bordercount, Marshal.SizeOf( typeof( GridVertex ) ), ComputeBufferType.Append );
        _cmptActiveBorderIDs.SetCounterValue( (uint)ActiveHexagons.Count );
        _cmptActiveBorderVertices.SetCounterValue( 0 );
        _cmptActiveBorderIDs.SetData( ActiveHexagons.ToArray() );

        int kernelHandleBuildBorder = ComputeTerrainShader.FindKernel( "BuildActiveBorder" );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "HexInput", _cmptBufferHexagons );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "BorderVertices", _cmptBorderOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "ActiveBorderIDs", _cmptActiveBorderIDs );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "ActiveBorderVertices", _cmptActiveBorderVertices );
        ComputeTerrainShader.Dispatch( kernelHandleBuildBorder, HexagonCount, 1, 1 );

        MatTerrain.SetBuffer( "Vertices", _cmptBufferOut );
        Mat_Border.SetBuffer( "BVertices", _cmptActiveBorderVertices );

        GridVertex[] debugBorder = new GridVertex[bordercount];
        _cmptActiveBorderVertices.GetData( debugBorder );
        for ( int i = 0; i < debugBorder.Length; i++ )
        {
            Debug.Log( "Border: " + debugBorder[i].vertex );

        }

        uint[] debugIds = new uint[ActiveHexagons.Count];
        _cmptActiveBorderIDs.GetData( debugIds );
        for ( int i = 0; i < debugIds.Length; i++ )
        {
            Debug.Log( "ID: " + debugIds[i]);

        }

        Debug.Log( "Rendered " + bordercount.ToString() + " Border Vertices" );
    }


    private int GetVertexCount()
    {

        return HexagonCount * 96;


        //int count = 0;

        //for ( int i = 0; i < _hexBuffer.GetLength( 0 ); i++ )
        //{
        //    for ( int j = 0; j < _hexBuffer.GetLength( 1 ); j++ )
        //    {
        //        Vector4 tesselation = _hexBuffer[i, j].tesselation;
        //        if ( tesselation.x == 1 ) count += 6 * 3;
        //        else if ( tesselation.y == 1 ) count += 3 * ( ( 2 * 4 ) + ( 2 * 2 ) );
        //        else if ( tesselation.z == 1 ) count += 3 * ( ( 2 * 12 ) + ( 2 * 4 ) );
        //        // else if ( tesselation.w == 1 ) MATHS GO HERE 
        //    }
        //}

        //return count;
    }

    private int GetBorderVCount()
    {
        return HexagonCount * 36;
    }

    private int GetActiveBorderVCount()
    {
        int count = 0;
        for ( int i = 0; i < _hexagons.GetLength( 0 ); i++ )
        {
            for ( int j = 0; j < _hexagons.GetLength( 0 ); j++ ) count += _hexagons[i, j].NeighbourConnections * 6;
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
        if ( _cmptBorderOut != null ) _cmptBorderOut.Dispose();
        if ( _cmptActiveBorderIDs != null ) _cmptActiveBorderIDs.Dispose();
        if ( _cmptActiveBorderVertices != null ) _cmptActiveBorderVertices.Dispose();
    }
}
