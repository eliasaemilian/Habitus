#ifndef SHADER_INCLUDE
#define SHADER_INCLUDE

void AddTessFullQuadComp( inout uint n, float3 t, float3 r, float3 d, float3 l, float3 u ) // right, down, left, up with t as middle point
{
    //Vertices[n].pos = t;
    //Vertices[n + 1].pos = r;
    //Vertices[n + 2].pos = d;
    //n += 3;
    //Vertices[n].pos = t;
    //Vertices[n + 1].pos = d;
    //Vertices[n + 2].pos = l;
    //n += 3;
    //Vertices[n].pos = t;
    //Vertices[n + 1].pos = l;
    //Vertices[n + 2].pos = u;
    //n += 3;
    //Vertices[n].pos = t;
    //Vertices[n + 1].pos = u;
    //Vertices[n + 2].pos = r;
    //n += 3;
    
}



void AddTessFullQuadF2( inout uint n, float3 t, float3 te[8] ) // right, down, left, up with t as middle point
{
    //for ( uint i = 0; i < 7; i++ )
    //{
    //    Vertices[n].pos = t;
    //    Vertices[n + 1].pos = te[i];
    //    Vertices[n + 2].pos = te[i + 1];
    //    n += 3;
    //}
    //Vertices[n].pos = t;
    //Vertices[n + 1].pos = te[i];
    //Vertices[n + 2].pos = te[0];
    //n += 3;
}
#endif