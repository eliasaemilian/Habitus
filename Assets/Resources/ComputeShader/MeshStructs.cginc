#ifndef SHADER_INCLUDE
#define SHADER_INCLUDE

struct Vertex
{
    float4 pos; //xyz = pos, w = terrain type
};

struct Triangle
{
    Vertex v[3];
};

struct Hexagon
{
    float4 center; //xyz = center pos, w = terrain type
    float4 tesselation;
    float4 topverts[6]; //xyz = topverts, w = connected to neighbour
};

#endif