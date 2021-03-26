using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlaythroughInit : MonoBehaviour
{
    [SerializeField] private Config_Map _mapConfig = null;

    private void Awake()
    {
        // take config
        MapGeneration.GenerateTerrainTypes( _mapConfig );

        // setup the grid master
        // setup dependencies <- this needs to be in general startup
        // kill self
    }


}
