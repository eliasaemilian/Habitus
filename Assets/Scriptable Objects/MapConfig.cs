using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to save generated maps
/// </summary>
[CreateAssetMenu( fileName = "MapConfig", menuName ="Configs/Map")]
public class MapConfig : ScriptableObject
{
    public int GridSize;
    [Range(0, 1)] public float Mountyness;
}
