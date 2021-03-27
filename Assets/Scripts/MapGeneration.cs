using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapGeneration
{
    public static TerrainType[,] GenerateTerrainTypes( Config_Map config )
    {
        TerrainType[,] tileTypes = CreateNewBlankTerrain( config );

        if ( config.Mountyness > 0 ) ComputeMountains( config, ref tileTypes );


        return tileTypes;
    }

    private static TerrainType[,] CreateNewBlankTerrain( Config_Map config )
    {
        if (config.BlankTerrain == null)
        {
#if UNITY_EDITOR
            Debug.LogError( $"At least 1 Blank Terrain Type needs to be added to any Map Config. Please add a Blank Terrain to {config.MapName}" );
#endif
            return null;
        }

        TerrainType[,] types = new TerrainType[config.GridSize, config.GridSize];

        for ( int i = 0; i < types.GetLength( 0 ); i++ )
        {
            for ( int j = 0; j < types.GetLength( 1 ); j++ )
            {
                types[i, j] = new TerrainType( config.BlankTerrain );
            }
        }

        return types;
    }

    private static void ComputeMountains( Config_Map config, ref TerrainType[,] types )
    {
        // 1st pass, set main mountain tiles
        for ( int x = 0; x < types.GetLength( 0 ); x++ )
        {
            for ( int y = 0; y < types.GetLength( 1 ); y++ )
            {
                float px = x / (float)config.GridSize;
                float py = y / (float)config.GridSize;
                float prob = Mathf.PerlinNoise( px, py ) * config.Mountyness;

                float r = Random.Range( 0f, 1f );

                if ( r < prob ) types[x, y] = new TerrainType( config.MountainTerrain );
                //  else tileTypes[x, y] = TileType.blank;
                // Debug.Log( $"X: {x}, Y: {y} : Mountain Prob is {prob}" );
            }
        }

    }

}
