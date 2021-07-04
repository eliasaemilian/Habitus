using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "Terrain", menuName = "Configs/Map" )]
public class Config_Terrain : ScriptableObject {
    public string TerrainName;
    public int ID;
    [Range( 0, 3 )] public int Tesselation;

    //undefined, // 0
    //blank, // 1
    //mountain,
    //plane,
    //forest,
    //river,
    //lake

}

public class TerrainType {
    private int _id;
    public int ID { get { return _id; } }

    private int _tesselation;
    public int Tesselation { get { return _tesselation; } }

    public TerrainType( Config_Terrain config ) {
        _id = config.ID;
        _tesselation = config.Tesselation;
    }

}
