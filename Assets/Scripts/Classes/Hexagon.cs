using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Hexagon
{
    private TerrainType _type;
    public TerrainType Type { get { return _type; } }
    public Vector3 Center;

    public GPU gpu { get { return new GPU( Center, GetGPUTesselationFromType(), Type.ID ); } }

    public struct GPU
    {
        public Vector4 center;
        public Vector4 tesselation;

        public GPU( Vector3 c, Vector4 t, int id )
        {
            center = new Vector4( c.x, c.y, c.z, id );
            tesselation = t;
        }
    };


    private Vector4 GetGPUTesselationFromType()
    {
        if ( Type.Tesselation == 0 ) return new Vector4( 1, 0, 0, 0 );
        if ( Type.Tesselation == 1 ) return new Vector4( 0, 1, 0, 0 );
        if ( Type.Tesselation == 2 ) return new Vector4( 0, 0, 1, 0 );
        if ( Type.Tesselation == 3 ) return new Vector4( 0, 0, 0, 1 );

#if UNITY_EDITOR
        Debug.LogError( $"Could not generate Tesselation Factor for Hexagon Tile of Type {Type.ID} at {Center}" );
#endif
        return Vector4.zero;
    }

    public void ChangeTerrainType(TerrainType type)
    {
        _type = type;
    }
}


