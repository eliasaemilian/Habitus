using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;


[Serializable]
public class TerrainRenderer {

    public List<uint> ActiveHexagons = new List<uint>(); // DEBUG: LATER SET BUFFER DIRECTLY

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

    private ComputeBuffer _bufVerticesOut, _bufVerticesOutALT;
    private ComputeBuffer _bufHexagonInput, _bufBorderVerticesOut;

    private ComputeBuffer _bufActiveIDs, _bufRemoveIDs, _bufAddIDs, _bufChangeIDs;
    private ComputeBuffer _bufActiveBorderVertices, _bufActiveBorderTriangles;

    private Vector4 _size;
    private int kernelHandleBuildBorder;
    public TerrainRenderer( int gridSize, float tileHeight, float tileWidth, Setup_Render setup ) {
        _matTerrain = setup.Mat_Terrain;
        _computeTerrainShader = setup.Compute_Grid;

        _matBorder = setup.Mat_Border;

        _hexagons = new Hexagon[gridSize, gridSize];
        _hexBuffer = new Hexagon.GPU[gridSize, gridSize];

        _size = new Vector4( gridSize, gridSize, tileHeight * 0.5f, tileWidth * 0.5f );
    }

    private void Init() {
        FindKernelHandles();
        SetComputeBuffers();
        SetShaderParams();
    }

    int kernelHandle, kernelHandleBorder;
    private void FindKernelHandles() {
        kernelHandle = ComputeTerrainShader.FindKernel( "GenGridVertices" );
        kernelHandleBorder = ComputeTerrainShader.FindKernel( "GenMeshBorder" );
        kernelHandleBuildBorder = ComputeTerrainShader.FindKernel( "BuildActiveBorder" );

    }

    private void SetComputeBuffers() {
        ComputeTerrainShader.SetBuffer( kernelHandle, "HexInput", _bufHexagonInput );
        ComputeTerrainShader.SetBuffer( kernelHandle, "Vertices", _bufVerticesOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBorder, "BorderVertices", _bufBorderVerticesOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBorder, "HexInput", _bufHexagonInput );
    }

    private void SetShaderParams() {
        ComputeTerrainShader.SetVector( "_Size", _size );
    }

    public void ComputeVertexData() {
        CleanUp();

        if ( ActiveHexagons.Count <= 0 ) return;
        if ( VerticesCount <= 0 ) return;

        _bufVerticesOut = new ComputeBuffer( VerticesCount, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );
        _bufHexagonInput = new ComputeBuffer( (int)( _size.x * _size.y ), Marshal.SizeOf( typeof( Hexagon.GPU ) ), ComputeBufferType.Default );
        _bufBorderVerticesOut = new ComputeBuffer( VCountBorder, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );
        _bufHexagonInput.SetData( _hexBuffer );

        Init();


        int count = ( ( (int)_size.x * (int)_size.y ) + 63 ) / 64;

        Debug.Log( "Dispatch Vert Data with " + count );
        ComputeTerrainShader.Dispatch( kernelHandle, count, count, 1 );
        ComputeTerrainShader.Dispatch( kernelHandleBorder, count / gpuUtils.NUM_THREADS, count / gpuUtils.NUM_THREADS, 1 );


        RenderProceduralActiveHexagons();

    }


    private void ComputeActiveBorder() {

        int bordercount = GetActiveBorderVCount();
        if ( bordercount <= 0 ) return;

        if ( _bufActiveBorderVertices != null ) _bufActiveBorderVertices.Dispose();
        if ( _bufActiveBorderTriangles != null ) _bufActiveBorderTriangles.Dispose();

        _bufActiveBorderVertices = new ComputeBuffer( bordercount, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );
        _bufActiveBorderTriangles = new ComputeBuffer( bordercount / 3, Marshal.SizeOf( typeof( gpuUtils.Triangle ) ), ComputeBufferType.Append );

        _bufActiveIDs.SetCounterValue( (uint)ActiveHexagons.Count );
        _bufActiveBorderVertices.SetCounterValue( 0 );

        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "HexInput", _bufHexagonInput );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "BorderVertices", _bufBorderVerticesOut );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "ActiveIDs", _bufActiveIDs );
        ComputeTerrainShader.SetBuffer( kernelHandleBuildBorder, "ActiveBorderTriangles", _bufActiveBorderTriangles );

        int count = ( ActiveHexagons.Count + 63 ) / 64;
        ComputeTerrainShader.Dispatch( kernelHandleBuildBorder, count, count, 1 );

        int activeBorderVertBuffer = Shader.PropertyToID( "BVertices" );
        gpuUtils.TrisToVerts( ref _bufActiveBorderTriangles, ref _bufActiveBorderVertices, count );
        Mat_Border.SetBuffer( activeBorderVertBuffer, _bufActiveBorderVertices );
    }

    private void SetActiveIDBuffer() {

        if ( ActiveHexagons.Count <= 0 ) return;

        if ( _bufActiveIDs != null ) _bufActiveIDs.Dispose();

        _bufActiveIDs = new ComputeBuffer( ActiveHexagons.Count, sizeof( uint ), ComputeBufferType.Append );
        _bufActiveIDs.SetCounterValue( (uint)ActiveHexagons.Count );
        _bufActiveIDs.SetData( ActiveHexagons.ToArray() );

    }


    public void RenderProceduralActiveHexagons() {
        SetActiveIDBuffer();
        ComputeActiveHexagons();
        //ComputeActiveBorder();
    }

    public void RemoveHexagon( uint[] ids ) {
        SetToRemoveBuffer( ids );
        ComputeRemoveHexagons( ids.Length );
        //SetActiveIDBuffer();
        //ComputeActiveBorder();
    }

    public void AddHexagon( uint[] ids ) {
        SetToAddBuffer( ids );
        ComputeAddHexagons( ids.Length );
        //SetActiveIDBuffer();
        //ComputeActiveBorder();
    }
    public void ChangeHexagon( uint[] ids, uint[] terrain ) {

        Vector2[] data = new Vector2[ids.Length];
        for ( int i = 0; i < ids.Length; i++ ) {
            data[i] = new Vector2( ids[i], terrain[i] );
        }

        SetToChangeBuffer( data );
        ComputeChangeHexagons( data.Length );
        //SetActiveIDBuffer();
        //ComputeActiveBorder();
    }


    private void ComputeActiveHexagons() {
        if ( _bufVerticesOutALT != null ) _bufVerticesOutALT.Dispose();

        _bufActiveIDs.SetCounterValue( (uint)ActiveHexagons.Count );


        _bufAddIDs = new ComputeBuffer( ActiveHexagons.Count, sizeof( uint ), ComputeBufferType.Append );
        _bufAddIDs.SetCounterValue( (uint)ActiveHexagons.Count );
        _bufAddIDs.SetData( ActiveHexagons.ToArray() );
        _bufAddIDs.SetCounterValue( (uint)ActiveHexagons.Count );

        _bufVerticesOutALT = new ComputeBuffer( ActiveHexagons.Count * 96, Marshal.SizeOf( typeof( gpuUtils.GridVertex ) ), ComputeBufferType.Default );


        int kernel = ComputeTerrainShader.FindKernel( "AppendHexagons" );
        ComputeTerrainShader.SetBuffer( kernel, "Vertices", _bufVerticesOut );
        ComputeTerrainShader.SetBuffer( kernel, "ActiveIDs", _bufActiveIDs );
        ComputeTerrainShader.SetBuffer( kernel, "ToAddHexIDs", _bufAddIDs );
        ComputeTerrainShader.SetBuffer( kernel, "VerticesAlt", _bufVerticesOutALT );

        int count = ( ActiveHexagons.Count + 63 ) / 64;


        Debug.Log( "Dispatch with " + count );
        // ComputeTerrainShader.Dispatch( kernel, count, count, 1 );
        ComputeTerrainShader.Dispatch( kernel, ActiveHexagons.Count, 1, 1 );

        MatTerrain.SetBuffer( "Vertices", _bufVerticesOutALT );
    }

    private void SetToRemoveBuffer( uint[] ids ) { // TODO: combine
        if ( _bufRemoveIDs != null ) _bufRemoveIDs.Dispose();

        _bufRemoveIDs = new ComputeBuffer( ids.Length, sizeof( uint ), ComputeBufferType.Append );
        _bufRemoveIDs.SetData( ids );

    }

    private void SetToAddBuffer( uint[] ids ) {
        if ( _bufAddIDs != null ) _bufAddIDs.Dispose();

        _bufAddIDs = new ComputeBuffer( ids.Length, sizeof( uint ), ComputeBufferType.Append );
        _bufAddIDs.SetData( ids );

    }

    private void SetToChangeBuffer( Vector2[] ids ) {
        if ( _bufChangeIDs != null ) _bufChangeIDs.Dispose();

        _bufChangeIDs = new ComputeBuffer( ids.Length, Marshal.SizeOf( typeof( Vector2 ) ), ComputeBufferType.Append );
        _bufChangeIDs.SetData( ids );

    }

    private void ComputeRemoveHexagons( int count ) {

        _bufRemoveIDs.SetCounterValue( (uint)count );

        int kernel = ComputeTerrainShader.FindKernel( "RemoveHexagons" );
        ComputeTerrainShader.SetBuffer( kernel, "ToRemoveHexIDs", _bufRemoveIDs );
        ComputeTerrainShader.SetBuffer( kernel, "VerticesAlt", _bufVerticesOutALT );
        ComputeTerrainShader.Dispatch( kernel, count, 1, 1 );
    }

    private void ComputeAddHexagons( int count ) {

        _bufAddIDs.SetCounterValue( (uint)count );

        int kernel = ComputeTerrainShader.FindKernel( "AppendHexagons" );
        ComputeTerrainShader.SetBuffer( kernel, "ToAddHexIDs", _bufAddIDs );
        ComputeTerrainShader.SetBuffer( kernel, "Vertices", _bufVerticesOut );
        ComputeTerrainShader.SetBuffer( kernel, "VerticesAlt", _bufVerticesOutALT );

        ComputeTerrainShader.Dispatch( kernel, count, 1, 1 );
    }

    private void ComputeChangeHexagons( int count ) {

        _bufChangeIDs.SetCounterValue( (uint)count );

        int kernel = ComputeTerrainShader.FindKernel( "ChangeHexagons" );
        ComputeTerrainShader.SetBuffer( kernel, "ToChangeHexIDs", _bufChangeIDs );
        ComputeTerrainShader.SetBuffer( kernel, "Vertices", _bufVerticesOut );
        ComputeTerrainShader.SetBuffer( kernel, "VerticesAlt", _bufVerticesOutALT );

        ComputeTerrainShader.Dispatch( kernel, count, 1, 1 );
    }

    #region serialization
    // SERIALIZE DATA
    public Vector2[] SerializeFromGPU() {
        int kernel = ComputeTerrainShader.FindKernel( "SerializeMeshData" );

        ComputeBuffer readback = new ComputeBuffer( HexagonCount, Marshal.SizeOf( typeof( Vector2 ) ), ComputeBufferType.Default );

        ComputeTerrainShader.SetBuffer( kernel, "ActiveIDs", _bufActiveIDs );
        ComputeTerrainShader.SetBuffer( kernel, "CPUReadback", readback );
        ComputeTerrainShader.SetBuffer( kernel, "VerticesAlt", _bufVerticesOutALT );

        ComputeTerrainShader.Dispatch( kernel, HexagonCount, 1, 1 );


        Vector2[] data = new Vector2[HexagonCount];
        readback.GetData( data );
        readback.Release();

        return data;
    }
    #endregion
    private int GetVertexCount() {

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

    private int GetBorderVCount() {
        return HexagonCount * 36;
    }

    private int GetActiveBorderVCount() {
        int count = 0;
        for ( int i = 0; i < _hexagons.GetLength( 0 ); i++ ) {
            for ( int j = 0; j < _hexagons.GetLength( 0 ); j++ ) count += _hexagons[i, j].NeighbourConnections * 6;
        }
        return count;
    }


    public void AddHexagonToRenderer( int x, int y, Hexagon h ) {
        _hexagons[x, y] = h;
        _hexBuffer[x, y] = h.gpu;
    }

    public void CleanUp() {
        if ( _bufVerticesOut != null ) _bufVerticesOut.Dispose();
        if ( _bufHexagonInput != null ) _bufHexagonInput.Dispose();
        if ( _bufBorderVerticesOut != null ) _bufBorderVerticesOut.Dispose();
        if ( _bufActiveIDs != null ) _bufActiveIDs.Dispose();
        if ( _bufActiveBorderVertices != null ) _bufActiveBorderVertices.Dispose();
        if ( _bufActiveBorderTriangles != null ) _bufActiveBorderTriangles.Dispose();
        if ( _bufVerticesOutALT != null ) _bufVerticesOutALT.Dispose();
        if ( _bufRemoveIDs != null ) _bufRemoveIDs.Dispose();
        if ( _bufChangeIDs != null ) _bufChangeIDs.Dispose();
        if ( _bufAddIDs != null ) _bufAddIDs.Dispose();
    }
}
