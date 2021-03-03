using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[RequireComponent (typeof(MeshRenderer))] 
public class GridRenderer : MonoBehaviour
{
    private ComputeBuffer gridVertBuffer, sidesBuffer, topBuffer;
    [SerializeField] private Material Mat_GridLines;
    [SerializeField] private Material Mat_GridTop;
    [SerializeField] private Material Mat_GridSides;

    private Dictionary<Camera, CommandBuffer> camsRaster = new Dictionary<Camera, CommandBuffer>();
    private Dictionary<Camera, CommandBuffer> camsMesh = new Dictionary<Camera, CommandBuffer>();

    int indicesTotal, indicesSides, indicesTop;
    public bool gridOff;

    private Grid grid;

    private void OnToggleGrid()
    {
        gridOff = !gridOff;
        if ( !gridOff ) InitGridLinesBuffer();
        else DisposeGridRasterBuffer();
    }

    void Start()
    {
        if ( FindObjectOfType<CameraController>() != null ) CameraController.CameraUpdate.AddListener( RefreshGridLines );

        // Toggle Grid
        if ( FindObjectOfType<InterfaceListener>() != null) InterfaceListener.ToggleGrid.AddListener( OnToggleGrid );

        Init();
    }

    public void Init()
    {
        grid = GetComponent<GridMaster>().Grid;

        InitGridLinesBuffer();
        InitGridSidesBuffer();
    }

    public void DebugReset()
    {
        OnDisable();
    }

    private void InitGridLinesBuffer()
    {
        
        if ( grid.GridVertsOut != null ) indicesTotal = grid.GridVertsOut.Length;

        if ( indicesTotal <= 0 ) return;

        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();
        gridVertBuffer = new ComputeBuffer( indicesTotal, sizeof( float ) * 3, ComputeBufferType.Default );
        gridVertBuffer.SetData( grid.GridVertsOut );

        Mat_GridLines.SetBuffer( "VertBuffer", gridVertBuffer );
    }

    private void InitGridSidesBuffer()
    {
        Grid grid = GetComponent<GridMaster>().Grid;
        indicesSides = grid.VerticesSides.Length;

        if ( indicesSides <= 0 ) return;

        if ( sidesBuffer != null ) sidesBuffer.Dispose();
        sidesBuffer = new ComputeBuffer( indicesSides, sizeof( float ) * 3, ComputeBufferType.Default );
        sidesBuffer.SetData( grid.VerticesSides );

        Mat_GridSides.SetBuffer( "VertBuffer", sidesBuffer );
    }




    private void OnDisable()
    {
       
        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();
        if ( sidesBuffer != null ) sidesBuffer.Dispose();
        if ( topBuffer != null ) topBuffer.Dispose();

        grid.TestTerrainGreen.Cleanup();
        grid.TestTerrainMountain.Cleanup();

        foreach ( var camera in camsRaster )
        {
            if ( camera.Key ) camera.Key.RemoveCommandBuffer( CameraEvent.AfterEverything, camera.Value );
        }

        foreach ( var camera in camsMesh )
        {
            if ( camera.Key ) camera.Key.RemoveCommandBuffer( CameraEvent.BeforeForwardOpaque, camera.Value );
        }

        camsRaster.Clear();
        camsMesh.Clear();

    }

    public void RefreshGridLines()
    {
        DisposeGridRasterBuffer();
        InitGridLinesBuffer();
        Debug.Log( "Update Grid" );
    }

    private void DisposeGridRasterBuffer()
    {
        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();

        foreach ( var camera in camsRaster )
        {
            if ( camera.Key ) camera.Key.RemoveCommandBuffer( CameraEvent.AfterEverything, camera.Value );
        }

        camsRaster.Clear();
    }

    private void OnWillRenderObject()
    {
        bool isActive = gameObject.activeInHierarchy && enabled;
        if ( !isActive )
        {
            OnDisable();
            return;
        }

        var cam = Camera.current;
        if ( !cam ) return;

        DrawMesh( cam );
        DrawRaster( cam );


    }

    private void DrawMesh( Camera cam )
    {
        if ( camsRaster.ContainsKey( cam ) ) return;

        // Init Cmd Buffer
        CommandBuffer cmdMesh = new CommandBuffer();
        cmdMesh.name = "Grid Mesh";
        camsMesh[cam] = cmdMesh;

        // Draw Terrain
        DrawTerrainMesh( grid.TestTerrainGreen, cmdMesh );
        DrawTerrainMesh( grid.TestTerrainMountain, cmdMesh );

        // Draw Border
        // ---

        // Add Cmd Buffer
        cam.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, cmdMesh );
    }

    private void DrawTerrainMesh( TerrainRenderer renderer, CommandBuffer buf )
    {
        renderer.Mat_Terrain.SetPass( 0 );
        buf.DrawProcedural( transform.localToWorldMatrix, renderer.Mat_Terrain, -1, MeshTopology.Triangles, renderer.Vertices.Count, 1 );
    }

    private void DrawRaster(Camera cam)
    {
        if ( camsRaster.ContainsKey( cam ) ) return;


        if ( !gridOff )
        {
            CommandBuffer cmdRaster = new CommandBuffer();
            cmdRaster.name = "Grid Raster";
            Mat_GridLines.SetPass( 0 );

            camsRaster[cam] = cmdRaster;
            cmdRaster.DrawProcedural( transform.localToWorldMatrix, Mat_GridLines, -1, MeshTopology.Lines, indicesTotal, 1 );
            cam.AddCommandBuffer( CameraEvent.AfterEverything, cmdRaster );
        }

    }

}
