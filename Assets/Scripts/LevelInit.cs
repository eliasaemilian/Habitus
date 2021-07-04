using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInit : MonoBehaviour {

    private Grid _grid;
    public int[] LaunchNewPlaythrough( Setup_Render renderSetup, Config_Map mapConfig ) {


        // take config
        _grid = InitGrid( renderSetup, mapConfig );

        // setup the grid master
        InitGridMaster( _grid );

        // init Grid Renderer
        InitGridRenderer( _grid );

        // setup dependencies 
        _grid.GenerateProceduralGrid();

        // get terrain types from seed ( <- TODO: use computed seed & save seed )
        return _grid.GetTerrainTypeIDs;
    }

    public void LaunchFromSavefile( MapSave savefile, TerrainType[,] terrain, uint[] activeIDs ) {

        // take config
        _grid = InitGrid( savefile.renderSetup, savefile.mapConfig, terrain );

        // setup the grid master
        InitGridMaster( _grid ); // TODO: insert new terrain types [DONE] & set active hexagons to saved [DONE]

        // init Grid Renderer
        InitGridRenderer( _grid );

        // setup dependencies
        _grid.GenerateProceduralGrid();

        SetActiveHexagons( activeIDs );

    }

    private Grid InitGrid( Setup_Render renderSetup, Config_Map mapConfig, TerrainType[,] terrain = null ) {
        Grid grid = new Grid( renderSetup, mapConfig, terrain );

        transform.position = Vector3.zero;
        grid.DebugWorldMatrix = transform.localToWorldMatrix;

        return grid;
    }

    private void InitGridMaster( Grid grid ) {
        GameObject gridMaster = new GameObject();

        gridMaster.name = "GridMaster";
        gridMaster.transform.position = Vector3.zero;
        gridMaster.AddComponent<GridMaster>().CreateComponent( grid );

    }


    private void InitGridRenderer( Grid grid ) {
        GameObject gridRenderer = new GameObject();

        gridRenderer.name = "GridRenderer";
        gridRenderer.transform.position = Vector3.zero;
        gridRenderer.AddComponent<GridRenderer>().CreateComponent( grid );
    }

    public void SetActiveHexagons( uint[] ids ) {
        _grid.ActiveHexagons.Clear();
        _grid.ActiveHexagons.AddRange( ids );
    }
}
