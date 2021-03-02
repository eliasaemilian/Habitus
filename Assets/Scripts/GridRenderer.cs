using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class TerrainRenderer
{
    public Material Mat_Terrain;
    public CommandBuffer Cmd_DrawTerrain;
    public ComputeShader Compute_Terrain;

    // FOR DEBUG
    public Vector3[] Vertices;
    public int[] Indices;
}


[RequireComponent (typeof(MeshRenderer))] 
public class GridRenderer : MonoBehaviour
{
    private TerrainRenderer TestTerrainGreen;
    private TerrainRenderer TestTerrainBlue;

    private ComputeBuffer gridVertBuffer, sidesBuffer, topBuffer;
    [SerializeField] private Material Mat_GridLines;
    [SerializeField] private Material Mat_GridTop;
    [SerializeField] private Material Mat_GridSides;

    private Dictionary<Camera, CommandBuffer> camsRaster = new Dictionary<Camera, CommandBuffer>();
    private Dictionary<Camera, CommandBuffer> camsMesh = new Dictionary<Camera, CommandBuffer>();

    int indicesTotal, indicesSides, indicesTop;
    public bool gridOff;
    private void OnToggleGrid()
    {
        gridOff = !gridOff;
        if ( !gridOff ) InitGridLinesBuffer();
        else DisposeGridRasterBuffer();
    }

    void Start()
    {
        // Toggle Grid
        if (FindObjectOfType<InterfaceListener>() != null) InterfaceListener.ToggleGrid.AddListener( OnToggleGrid );


        //
        InitGridLinesBuffer();
        InitGridSidesBuffer();
        InitGridTopPlaneBuffer();

    }

    private void InitGridLinesBuffer()
    {
        Grid grid = GetComponent<GridMaster>().Grid;
        indicesTotal = grid.GridVertsOut.Length;

        if ( indicesTotal <= 0 ) return;

        gridVertBuffer = new ComputeBuffer( indicesTotal, sizeof( float ) * 3, ComputeBufferType.Default );
        gridVertBuffer.SetData( grid.GridVertsOut );

        Mat_GridLines.SetBuffer( "VertBuffer", gridVertBuffer );
    }

    private void InitGridSidesBuffer()
    {
        Grid grid = GetComponent<GridMaster>().Grid;
        indicesSides = grid.VerticesSides.Length;

        if ( indicesSides <= 0 ) return;

        sidesBuffer = new ComputeBuffer( indicesSides, sizeof( float ) * 3, ComputeBufferType.Default );
        sidesBuffer.SetData( grid.VerticesSides );

        Mat_GridSides.SetBuffer( "VertBuffer", sidesBuffer );
    }

    private void InitGridTopPlaneBuffer()
    {
        Grid grid = GetComponent<GridMaster>().Grid;
        indicesTop = grid.VerticesTop.Length;

        if ( indicesTop <= 0 ) return;

        topBuffer = new ComputeBuffer( indicesTop, sizeof( float ) * 3, ComputeBufferType.Default );
        topBuffer.SetData( grid.VerticesTop );

        Mat_GridTop.SetBuffer( "VertBuffer", topBuffer );
    }

    private void OnDisable()
    {
       
        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();
        if ( sidesBuffer != null ) sidesBuffer.Dispose();
        if ( topBuffer != null ) topBuffer.Dispose();

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
        // if cmd buffer is already registered on camera, return
        if ( camsMesh.ContainsKey( cam ) ) return;

        CommandBuffer cmdMesh = new CommandBuffer();
        cmdMesh.name = "Grid Mesh";
        Mat_GridSides.SetPass( 0 );
        Mat_GridTop.SetPass( 0 );

        camsMesh[cam] = cmdMesh;
        cmdMesh.DrawProcedural( transform.localToWorldMatrix, Mat_GridTop, -1, MeshTopology.Triangles, indicesTop, 1 );
        cmdMesh.DrawProcedural( transform.localToWorldMatrix, Mat_GridSides, -1, MeshTopology.Triangles, indicesSides, 1 );

        cam.AddCommandBuffer( CameraEvent.BeforeForwardOpaque, cmdMesh );
    }

    private void DrawRaster(Camera cam)
    {
        // if cmd buffer is already registered on camera, return
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
