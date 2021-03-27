using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapGeneration
{
    public static TerrainType[,] GenerateTerrainTypes( Config_Map config )
    {
        TerrainType[,] tileTypes = new TerrainType[config.GridSize, config.GridSize];

        FillBlank(ref tileTypes, config);

        if ( config.Mountyness > 0 ) ComputeMountains( config );


        return tileTypes;
    }

    private static void FillBlank( ref TerrainType[,] terrainTypes, Config_Map config )
    {
        for ( int i = 0; i < terrainTypes.GetLength(0); i++ )
        {
            for ( int j = 0; j < terrainTypes.GetLength( 1 ); j++ )
            {
                terrainTypes[i, j] = new TerrainType( config.Terrains[0] );
            }
        }
    }

    private static void ComputeMountains( Config_Map MapConfig )
    {
        // 1st pass, set main mountain tiles
        for ( int x = 0; x < MapConfig.GridSize; x++ )
        {
            for ( int y = 0; y < MapConfig.GridSize; y++ )
            {
                float px = x / (float)MapConfig.GridSize;
                float py = y / (float)MapConfig.GridSize;
                float prob = Mathf.PerlinNoise( px, py ) * MapConfig.Mountyness;

                float r = Random.Range( 0f, 1f );
                
              //  if ( r < prob ) tileTypes[x, y] = TileType.mountain;
              //  else tileTypes[x, y] = TileType.blank;
               // Debug.Log( $"X: {x}, Y: {y} : Mountain Prob is {prob}" );
            }
        }

    }

}
