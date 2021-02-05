using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent (typeof(MeshRenderer))]
public class GridRenderer : MonoBehaviour
{
    private ComputeBuffer gridVertBuffer;
    [SerializeField] private Material Mat_GridLines;

    private Dictionary<Camera, CommandBuffer> cams = new Dictionary<Camera, CommandBuffer>();

    int indicesTotal;

    void Start()
    {
        //  InitCommandBufferMat();
        Grid grid = GetComponent<GridMaster>().Grid;
        indicesTotal = grid.GridVertsOut.Length;
        gridVertBuffer = new ComputeBuffer( indicesTotal, sizeof( float ) * 3, ComputeBufferType.Default );
        gridVertBuffer.SetData( grid.GridVertsOut );
        
        Mat_GridLines.SetBuffer( "VertBuffer", gridVertBuffer );
    }

    private void OnDisable()
    {
        if ( gridVertBuffer != null ) gridVertBuffer.Dispose();

        foreach ( var camera in cams )
        {
            if ( camera.Key ) camera.Key.RemoveCommandBuffer( CameraEvent.AfterEverything, camera.Value );
        }

        cams.Clear();

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

        // clear cmd buffer
        CommandBuffer cmd = null;

        // if cmd buffer is already registered on camera, return
        if ( cams.ContainsKey( cam ) ) return;

        cmd = new CommandBuffer();
        cmd.name = "Buffer Test";
        Mat_GridLines.SetPass( 0 );

        cams[cam] = cmd;
        cmd.DrawProcedural( transform.localToWorldMatrix, Mat_GridLines, -1, MeshTopology.Lines, indicesTotal, 1 );

        cam.AddCommandBuffer( CameraEvent.AfterEverything, cmd );
    }

}
