using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    public MapConfig MapConfig;


    // Generated Values //
    private TileType[,] tileTypes;


    // Start is called before the first frame update
    void Awake()
    {
        
        // generate Mountain Tiles from Mountyness
        tileTypes = new TileType[MapConfig.GridSize, MapConfig.GridSize];

        if ( MapConfig.Mountyness > 0 ) ComputeMountains();

        // pass to Grid
        GridMaster.Instance.InitGrid( MapConfig.GridSize, tileTypes );

    }

    private void ComputeMountains()
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

                if ( r < prob ) tileTypes[x, y] = TileType.mountain;
                else tileTypes[x, y] = TileType.blank;
               // Debug.Log( $"X: {x}, Y: {y} : Mountain Prob is {prob}" );
            }
        }

    }


    public void OnClickGenerateMap() => Awake();

}
