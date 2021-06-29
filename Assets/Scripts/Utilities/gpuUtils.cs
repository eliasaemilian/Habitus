using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class gpuUtils {

    public const int NUM_THREADS = 8;

    [System.Serializable]
    public struct GridVertex {
        public Vector4 vertex;
    }

    public struct Triangle {
        public GridVertex v1;
        public GridVertex v2;
        public GridVertex v3;
    }

    public static void TrisToVerts( ref ComputeBuffer bufIn, ref ComputeBuffer bufOut, int count ) {

        if ( !DispatchTrisToVertsCompute( ref bufIn, ref bufOut, count ) ) TrisToVertCPUAlt( ref bufIn, ref bufOut, count );
    }

    private static bool DispatchTrisToVertsCompute( ref ComputeBuffer bufIn, ref ComputeBuffer bufOut, int count ) {

        ComputeShader cmpt = Resources.Load<ComputeShader>( "ComputeShader/Cmpt_TrisToVert" );
        if ( cmpt == null ) {
#if UNITY_EDITOR
            Debug.LogError( "Failed to load ComputeShader Cmpt_TrisToVert from Resources" );
#endif
            return false;
        }

        int kernel = cmpt.FindKernel( "TrisToVerticesBuffer" );
        cmpt.SetBuffer( kernel, "ArgsIn", bufIn );
        cmpt.SetBuffer( kernel, "ArgsOut", bufOut );
        cmpt.Dispatch( kernel, count / 8, 1, 1 );
        return true;
    }

    private static void TrisToVertCPUAlt( ref ComputeBuffer bufIn, ref ComputeBuffer bufOut, int count ) {

        Triangle[] data = new Triangle[count / 3];
        GridVertex[] vertices = new GridVertex[count];
        bufIn.GetData( data );
        int n = 0;
        for ( int i = 0; i < count / 3; i++ ) {
            vertices[n] = data[i].v1;
            vertices[n + 1] = data[i].v2;
            vertices[n + 2] = data[i].v3;
            n += 3;
        }
        bufOut.SetData( vertices );
    }

}
