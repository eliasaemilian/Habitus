using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlaythroughInit : MonoBehaviour
{
    [SerializeField] private Config_Map _mapConfig = null;

    public Grid DebugGrid;

    private void Awake()
    {
        // take config
        Grid grid = InitGrid();

        // setup the grid master
        InitGridMaster( grid );

        // init Grid Renderer
        InitGridRenderer( grid );

        // setup dependencies <- this needs to be in general startup
        grid.GenerateProceduralGrid();
        // kill self
    }

    private Grid InitGrid()
    {
        Grid grid = new Grid( _mapConfig );

        transform.position = Vector3.zero;
        grid.DebugWorldMatrix = transform.localToWorldMatrix;



        return grid;
    }

    private void InitGridMaster(Grid grid )
    {
        GameObject gridMaster = new GameObject();
        
        gridMaster.name = "GridMaster";
        gridMaster.transform.position = Vector3.zero;
        gridMaster.AddComponent<GridMaster>().CreateComponent(gridMaster.transform, grid);

    }


    private void InitGridRenderer( Grid grid )
    {
        GameObject gridRenderer = new GameObject();

        gridRenderer.name = "GridRenderer";
        gridRenderer.transform.position = Vector3.zero;
        gridRenderer.AddComponent<GridRenderer>().CreateComponent( gridRenderer.transform, grid );
        
    }
}
