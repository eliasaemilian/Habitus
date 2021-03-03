using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

[Serializable]
public struct GridVertex
{
    public Vector3 vertex;
    public int terrainType; // = TerrainType
}

[Serializable]
public struct Hexagon
{
    public Vector3 center;
    public float width;
    public float height;
};

[RequireComponent (typeof(MeshRenderer))] 
public class GridRenderer : MonoBehaviour
{
    private ComputeBuffer gridVertBuffer, sidesBuffer, topBuffer;
    [SerializeField] private Material Mat_Raster;
    [SerializeField] private Material Mat_GridBorder;

    private Dictionary<Camera, CommandBuffer> camsRaster = new Dictionary<Camera, CommandBuffer>();
    private Dictionary<Camera, CommandBuffer> camsMesh = new Dictionary<Camera, CommandBuffer>();

    int indicesRaster, indicesBorder;
    public bool gridOff;

    private Grid grid;

    private void OnToggleGrid()
    {
        gridOff = !gridOff;
        if ( !gridOff ) InitRasterBuffer();
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

        InitRasterBuffer();
        InitGridBorderBuffer();
    }



    private void InitRasterBuffer()
    {        
        if ( grid.RasterVertices != null ) indicesRaster = grid.RasterVertices.Length;

        if ( indicesRaster <= 0 ) return;

        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();
        gridVertBuffer = new ComputeBuffer( indicesRaster, sizeof( float ) * 3, ComputeBufferType.Default );
        gridVertBuffer.SetData( grid.RasterVertices );

        Mat_Raster.SetBuffer( "VertBuffer", gridVertBuffer );
    }

    private void InitGridBorderBuffer()
    {
        Grid grid = GetComponent<GridMaster>().Grid;
        indicesBorder = grid.VerticesSides.Length;

        if ( indicesBorder <= 0 ) return;

        if ( sidesBuffer != null ) sidesBuffer.Dispose();
        sidesBuffer = new ComputeBuffer( indicesBorder, sizeof( float ) * 3, ComputeBufferType.Default );
        sidesBuffer.SetData( grid.VerticesSides );

        Mat_GridBorder.SetBuffer( "VertBuffer", sidesBuffer );
    }

    public void DebugReset()
    {
        OnDisable();
    }


    private void OnDisable()
    {       
        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();
        if ( sidesBuffer != null ) sidesBuffer.Dispose();
        if ( topBuffer != null ) topBuffer.Dispose();

        grid.TestTerrainGreen.Cleanup();
       // grid.TestTerrainMountain.Cleanup();

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
        InitRasterBuffer();
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


    // ------------------------------------- DRAW CALL ---------------------------------------- //
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
       // DrawTerrainMesh( grid.TestTerrainMountain, cmdMesh );

        // Draw Border
        DrawBorder( cmdMesh );

        // Add Cmd Buffer
        cam.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, cmdMesh );
    }

    private void DrawTerrainMesh( TerrainRenderer renderer, CommandBuffer buf )
    {
        Debug.Log( "Drawing Terrain " + renderer.Mat_Terrain);
        renderer.Mat_Terrain.SetPass( 0 );
        buf.DrawProcedural( transform.localToWorldMatrix, renderer.Mat_Terrain, -1, MeshTopology.Triangles, renderer.GridVertices.Count, 1 );
    }

    private void DrawBorder(CommandBuffer buf)
    {
        Mat_GridBorder.SetPass( 0 );
        buf.DrawProcedural( transform.localToWorldMatrix, Mat_GridBorder, -1, MeshTopology.Triangles, indicesBorder, 1 );
    }

    private void DrawRaster(Camera cam)
    {
        if ( camsRaster.ContainsKey( cam ) ) return;


        if ( !gridOff )
        {
            CommandBuffer cmdRaster = new CommandBuffer();
            cmdRaster.name = "Grid Raster";
            Mat_Raster.SetPass( 0 );

            camsRaster[cam] = cmdRaster;
            cmdRaster.DrawProcedural( transform.localToWorldMatrix, Mat_Raster, -1, MeshTopology.Lines, indicesRaster, 1 );
            cam.AddCommandBuffer( CameraEvent.AfterEverything, cmdRaster );
        }

    }

}
