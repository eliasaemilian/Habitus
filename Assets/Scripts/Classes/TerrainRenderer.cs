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
    private ComputeBuffer _cmptActiveIDs;
    private ComputeBuffer _cmptActiveBorderVertices;
    private ComputeBuffer _cmptActiveBorderTriangles;
    private ComputeBuffer _cmptVerticesAlt;
    private ComputeBuffer _cmptRemoveIDs;
    private Vector4 _size;


    public Hexagon.GPU[] DEBUGHEX;

    public TerrainRenderer( int gridSize, float tileHeight, float tileWidth, Setup_Render setup ) 
    {
        _matTerrain = setup.Mat_Terrain;
        _computeTerrainShader = setup.Compute_Grid; 

        _matBorder = setup.Mat_Border;

        _hexagons = new Hexagon[gridSize, gridSize];
        _hexBuffer = new Hexagon.GPU[gridSize, gridSize];

        _size = new Vector4( gridSize, gridSize, tileHeight * 0.5f, tileWidth * 0.5f );

    }


    public void ComputeVertexData()
    {
        CleanUp();

        if ( ActiveHexagons.Count <= 0 ) return;
        if ( VerticesCount <= 0 ) return;

        _cmptBufferOut = new ComputeBuffer( VerticesCount, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );
        _cmptBufferHexagons = new ComputeBuffer( (int)(_size.x * _size.y), Marshal.SizeOf(typeof(Hexagon.GPU)), ComputeBufferType.Default );
        _cmptBorderOut = new ComputeBuffer( VCountBorder, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );
        _cmptBufferHexagons.SetData( _hexBuffer );

        int kernelHandle = ComputeTerrainShader.FindKernel( "GenGridVertices" );
        int kernelHandleBorder = ComputeTerrainShader.FindKernel( "GenMeshBorder" );

        ComputeTerrainShader.SetBuffer( kernelHandle, "HexInput", _cmptBufferHexagons );
        ComputeTerrainShader.SetBuffer( kernelHandle, "Vertices", _cmptBufferOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBorder, "BorderVertices", _cmptBorderOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBorder, "HexInput", _cmptBufferHexagons );

        ComputeTerrainShader.SetVector( "_Size", _size );

        ComputeTerrainShader.Dispatch( kernelHandle, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, 1 );
        ComputeTerrainShader.Dispatch( kernelHandleBorder, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, ( (int)_size.x * (int)_size.y ) / NUM_THREADS, 1 );


        RenderProceduralActiveHexagons();

    }


    private void ComputeActiveBorder()
    {
        int bordercount = GetActiveBorderVCount();
        if ( bordercount <= 0 ) return;

        if ( _cmptActiveBorderVertices != null ) _cmptActiveBorderVertices.Dispose();
        if ( _cmptActiveBorderTriangles != null ) _cmptActiveBorderTriangles.Dispose();

        _cmptActiveBorderVertices = new ComputeBuffer( bordercount, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );
        _cmptActiveBorderTriangles = new ComputeBuffer( bordercount / 3, Marshal.SizeOf( typeof( gpuUtils.Triangle ) ), ComputeBufferType.Append );

        _cmptActiveIDs.SetCounterValue( (uint)ActiveHexagons.Count );
        _cmptActiveBorderVertices.SetCounterValue( 0 );

        int kernelHandleBuildBorder = ComputeTerrainShader.FindKernel( "BuildActiveBorder" );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "HexInput", _cmptBufferHexagons );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "BorderVertices", _cmptBorderOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "ActiveIDs", _cmptActiveIDs );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "ActiveBorderTriangles", _cmptActiveBorderTriangles );
        ComputeTerrainShader.Dispatch( kernelHandleBuildBorder, ActiveHexagons.Count / 8, 1, 1 );

        gpuUtils.TrisToVerts( ref _cmptActiveBorderTriangles, ref _cmptActiveBorderVertices, bordercount );
        Mat_Border.SetBuffer( "BVertices", _cmptActiveBorderVertices );
    }

    private void SetActiveIDBuffer()
    {
        if ( ActiveHexagons.Count <= 0 ) return;

        if ( _cmptActiveIDs != null ) _cmptActiveIDs.Dispose();

        _cmptActiveIDs = new ComputeBuffer( ActiveHexagons.Count, sizeof( uint ), ComputeBufferType.Append );
        _cmptActiveIDs.SetCounterValue( (uint)ActiveHexagons.Count );
        _cmptActiveIDs.SetData( ActiveHexagons.ToArray() );

    }

    public void RenderProceduralActiveHexagons()
    {
        SetActiveIDBuffer();
        ComputeActiveHexagons();
        ComputeActiveBorder();
    }

    public void RemoveHexagon( uint[] ids )
    {
        SetToRemoveBuffer( ids );
        ComputeRemoveHexagons( ids.Length );
        SetActiveIDBuffer();
        ComputeActiveBorder();
    }

    private void ComputeActiveHexagons()
    {
        if ( _cmptVerticesAlt != null ) _cmptVerticesAlt.Dispose();

        _cmptActiveIDs.SetCounterValue( (uint)ActiveHexagons.Count );
        _cmptVerticesAlt = new ComputeBuffer( ActiveHexagons.Count * 96, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );


        int kernel = ComputeTerrainShader.FindKernel( "AppendHexagons" );
        ComputeTerrainShader.SetBuffer( kernel, "Vertices", _cmptBufferOut );
        ComputeTerrainShader.SetBuffer( kernel, "ActiveIDs", _cmptActiveIDs );
        ComputeTerrainShader.SetBuffer( kernel, "VerticesAlt", _cmptVerticesAlt );
        ComputeTerrainShader.Dispatch( kernel, ActiveHexagons.Count / 8, 1, 1 );

        MatTerrain.SetBuffer( "Vertices", _cmptVerticesAlt );
    }

    private void SetToRemoveBuffer(uint[] ids)
    {
        if ( _cmptRemoveIDs != null ) _cmptRemoveIDs.Dispose();

        _cmptRemoveIDs = new ComputeBuffer( ids.Length, sizeof( uint ), ComputeBufferType.Append );
        _cmptRemoveIDs.SetData( ids );

    }

    private void ComputeRemoveHexagons( int count)
    {
       // if ( _cmptVerticesAlt != null ) _cmptVerticesAlt.Dispose();

        _cmptRemoveIDs.SetCounterValue( (uint)count );
        //_cmptVerticesAlt = new ComputeBuffer( ActiveHexagons.Count * 96, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );
        

        int kernel = ComputeTerrainShader.FindKernel( "RemoveHexagons" );
        ComputeTerrainShader.SetBuffer( kernel, "ToRemoveHexIDs", _cmptRemoveIDs );
        ComputeTerrainShader.SetBuffer( kernel, "VerticesAlt", _cmptVerticesAlt );
        ComputeTerrainShader.Dispatch( kernel, count, 1, 1 );

        MatTerrain.SetBuffer( "Vertices", _cmptVerticesAlt );
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
        if ( _cmptActiveIDs != null ) _cmptActiveIDs.Dispose();
        if ( _cmptActiveBorderVertices != null ) _cmptActiveBorderVertices.Dispose();
        if ( _cmptActiveBorderTriangles != null ) _cmptActiveBorderTriangles.Dispose();
        if ( _cmptVerticesAlt != null ) _cmptVerticesAlt.Dispose();
        if ( _cmptRemoveIDs != null ) _cmptRemoveIDs.Dispose();
    }
}
