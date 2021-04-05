using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Hexagon
{
    private TerrainType _type;
    public TerrainType Type { get { return _type; } }
    public Vector3 Center;

    public Vector3[] BorderVertices;

    public GPU gpu { get { return new GPU( Center, GetGPUTesselationFromType(), Type.ID ); } }

    public struct GPU
    {
        public Vector4 center;
        public Vector4 tesselation;

        public Vector3 topvert0;
        public Vector3 topvert1;
        public Vector3 topvert2;
        public Vector3 topvert3;
        public Vector3 topvert4;
        public Vector3 topvert5;

        public GPU( Vector3 c, Vector4 t, int id )
        {
            center = new Vector4( c.x, c.y, c.z, id );
            tesselation = t;
            topvert0 = Vector3.zero;
            topvert1 = Vector3.zero;
            topvert2 = Vector3.zero;
            topvert3 = Vector3.zero;
            topvert4 = Vector3.zero;
            topvert5 = Vector3.zero;
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

    //private Vector3[] GenBorderVertices( Cell cell, Vector3[] oVerts ) // TODO: REFACTOR
    //{
    //    Vector3 c = Center;
    //    float r = 0; float w = 0;
    //   // float y = Grid.TileThickness;

    //    Vector3 p1 = new Vector3( c.x + r, c.y, c.z ); // 0
    //    Vector3 p2 = new Vector3( c.x + r * .5f, c.y, c.z - w ); // 1
    //    Vector3 p3 = new Vector3( c.x - r * .5f, c.y, c.z - w ); // 2
    //    Vector3 p4 = new Vector3( c.x - r, c.y, c.z ); // 3
    //    Vector3 p5 = new Vector3( c.x - r * .5f, c.y, c.z + w ); // 4
    //    Vector3 p6 = new Vector3( c.x + r * .5f, c.y, c.z + w ); // 5

    //    // verts
    //    Vector3[] verts = new Vector3[24 + 2];
    //    int[] otherIndices = new int[( 3 * 24 ) + 36];
    //    int c = 1;
    //    //  Vector3 cTop = new Vector3( center.x, center.y + Grid.TileThickness, center.z ); // index 13
    //    Vector3 cBot = center; // index 0
    //    verts[0] = cBot;
    //    //       verts[13] = cTop;

    //    //     cell.Center = cTop;


    //    for ( int i = 0; i < oVerts.Length; i++ )
    //    {
    //        verts[c] = new Vector3( oVerts[i].x, 0, oVerts[i].z );
    //        verts[c + 1] = new Vector3( oVerts[i].x, 0, oVerts[i].z );
    //        c += 2;
    //    }
    //    c = 14;
    //    for ( int i = 0; i < oVerts.Length; i++ )
    //    {
    //        verts[c] = oVerts[i];
    //        verts[c + 1] = oVerts[i];
    //        c += 2;
    //    }


    //    // indices
    //    c = 0;

    //    // sides
    //    for ( int i = 0; i < 5; i++ )
    //    {
    //        otherIndices[c] = 15 + ( i * 2 );
    //        otherIndices[c + 1] = ( i * 2 ) + 2;
    //        otherIndices[c + 2] = 15 + ( i * 2 ) + 2;

    //        otherIndices[c + 3] = ( i * 2 ) + 2;
    //        otherIndices[c + 4] = ( i * 2 ) + 4;
    //        otherIndices[c + 5] = 15 + ( i * 2 ) + 2;

    //        c += 6;
    //    }

    //    otherIndices[c] = 12;
    //    otherIndices[c + 1] = 15;
    //    otherIndices[c + 2] = 25;

    //    otherIndices[c + 3] = 15;
    //    otherIndices[c + 4] = 12;
    //    otherIndices[c + 5] = 2;


    //    c += 6;
    //    // bottom hex plane
    //    //for ( int i = 1; i < 11; i += 2 )
    //    //{
    //    //    otherIndices[c] = 0;
    //    //    otherIndices[c + 1] = i + 2;
    //    //    otherIndices[c + 2] = i;
    //    //    c += 3;
    //    //}
    //    //otherIndices[c] = 0;
    //    //otherIndices[c + 1] = 1;
    //    //otherIndices[c + 2] = 11;

    //    cell.HVerts = verts;
    //    cell.HIndicesSides = otherIndices;

    //    return verts;
    //}

}


