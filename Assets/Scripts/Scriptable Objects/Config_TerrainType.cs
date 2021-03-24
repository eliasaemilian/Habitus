using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "TerrainType", menuName = "Configs/Terrain" )]
public class Config_TerrainType : ScriptableObject
{
    public string TerrainName;
    public int ID;
    [Range( 1, 4 )] public int Tesselation;

    //undefined, // 0
    //blank, // 1
    //mountain,
    //plane,
    //forest,
    //river,
    //lake
}

public class TerrainType
{
    private int _id;
    public int ID { get { return _id; } }

    private int _tesselation;
    public int Tesselation { get { return _tesselation; } }

    public TerrainType(Config_TerrainType config)
    {
        _id = config.ID;
        _tesselation = config.Tesselation;
    }
}
