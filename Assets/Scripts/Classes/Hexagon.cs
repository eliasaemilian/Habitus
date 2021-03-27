using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Hexagon
{
    public TerrainType Type;
    public Vector3 center;

    public GPU gpu { get { return new GPU( center, GetGPUTesselationFromType() ); } }

    public struct GPU
    {
        public Vector3 center;
        public Vector4 tesselation;

        public GPU(Vector3 c, Vector4 t)
        {
            center = c;
            tesselation = t;
        }
    };


    private Vector4 GetGPUTesselationFromType()
    {
        if ( Type.Tesselation == 1 ) return new Vector4( 1, 0, 0, 0 );
        if ( Type.Tesselation == 2 ) return new Vector4( 0, 1, 0, 0 );
        if ( Type.Tesselation == 3 ) return new Vector4( 0, 0, 1, 0 );
        if ( Type.Tesselation == 4 ) return new Vector4( 0, 0, 0, 1 );

#if UNITY_EDITOR
        Debug.LogError( $"Could not generate Tesselation Factor for Hexagon Tile of Type {Type.ID} at {center}" );
#endif
        return Vector4.zero;
    }


}


