using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

[Serializable]
public struct GridVertex
{
    public Vector4 vertex;
}

public struct Triangle
{
    public GridVertex v1;
    public GridVertex v2;
    public GridVertex v3;
}


[RequireComponent (typeof(MeshRenderer))] 
public class GridRenderer : InstantiatedGridComponent
{
    private ComputeBuffer gridVertBuffer;
    private Material Mat_Raster;

    private Dictionary<Camera, CommandBuffer> camsRaster = new Dictionary<Camera, CommandBuffer>();
    private Dictionary<Camera, CommandBuffer> camsMesh = new Dictionary<Camera, CommandBuffer>();

    int indicesRaster;
    public bool gridOff;

    private ComputeShader cmpt_grid;

    private void OnToggleGrid()
    {
        gridOff = !gridOff;
        if ( !gridOff ) InitRasterBuffer();
        else DisposeGridRasterBuffer();
    }


    protected override void Init()
    {
        //  if ( FindObjectOfType<CameraController>() != null ) CameraController.CameraUpdate.AddListener( RefreshGridLines );

        // Toggle Grid
        //   if ( FindObjectOfType<InterfaceListener>() != null ) InterfaceListener.ToggleGrid.AddListener( OnToggleGrid );

        //  InitRasterBuffer();
    }


    private void InitRasterBuffer()
    {        
        if ( Grid.RasterVertices != null && Grid.RasterVertices.Length > 0 ) indicesRaster = Grid.RasterVertices.Length;

        if ( indicesRaster <= 0 ) return;

        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();
        gridVertBuffer = new ComputeBuffer( indicesRaster, sizeof( float ) * 3, ComputeBufferType.Default );
        gridVertBuffer.SetData( Grid.RasterVertices );

        Mat_Raster.SetBuffer( "VertBuffer", gridVertBuffer );
    }


    public void DebugReset()
    {
        OnDisable();
    }


    private void OnDisable()
    {       
        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();

        Grid.CleanUp();

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


    // ------------------------------------- DRAW CALLS ---------------------------------------- //
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
      //  DrawRaster( cam );


    }
    private void DrawMesh( Camera cam )
    {
        if ( camsRaster.ContainsKey( cam ) ) return;

        // Init Cmd Buffer
        CommandBuffer cmdMesh = new CommandBuffer();
        cmdMesh.name = "Grid Mesh";
        camsMesh[cam] = cmdMesh;

        // Draw Terrain
        
        DrawTerrainMesh( cmdMesh );

        // Add Cmd Buffer
        cam.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, cmdMesh );
    }

    private void DrawTerrainMesh( CommandBuffer buf )
    {
        Grid.DrawTerrainProcedural( ref buf );
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
