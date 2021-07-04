using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializationHandler : MonoBehaviour {
    [SerializeField] private Config_Map _mapConfig = null;
    [SerializeField] private Setup_Render _renderSetup = null;

    public Grid DebugGrid;

    public bool forceFreshBuild;
    public MapSave debugSave;
    // Start is called before the first frame update
    void Start() {

        // load from save or generate new grid
        if ( forceFreshBuild ) {
            LoadNewPlaythrough();
        } else LoadFromSave( debugSave );


        // kill self
    }

    private void LoadFromSave( MapSave save ) {
        // Generate Grid from Save

        var init = gameObject.AddComponent<LevelInit>();
        init.LaunchFromSavefile( save, DeserializeTerrainTypes( save ), DeserializeActiveIDs( save ) );

    }


    private void LoadNewPlaythrough() {
        ClearSaveData();

        var init = gameObject.AddComponent<LevelInit>();
        SerializeSeed( init.LaunchNewPlaythrough( _renderSetup, _mapConfig ) );
    }

    private void SerializeSeed( int[] terrain ) {

        debugSave.mapConfig = _mapConfig;
        debugSave.renderSetup = _renderSetup;
        debugSave.originalTerrain = terrain;
    }

    private void ClearSaveData() {
        debugSave.originalTerrain = new int[0];
        debugSave.gpuData = new Vector2[0];
        debugSave.mapConfig = null;
        debugSave.renderSetup = null;
    }

    public void DebugSetGPUSaveData( Vector2[] data ) {
        debugSave.gpuData = data;
    }

    public TerrainType[,] DeserializeTerrainTypes( MapSave save ) {

        var mapConfig = save.mapConfig;
        var terrainTypes = new TerrainType[mapConfig.GridSize, mapConfig.GridSize];
        int count = 0;
        for ( int i = 0; i < terrainTypes.GetLength( 0 ); i++ ) {
            for ( int j = 0; j < terrainTypes.GetLength( 1 ); j++ ) {
                TerrainType type = new TerrainType( mapConfig.GetTerrainConfigByID( (int)save.gpuData[count].x ) );
                terrainTypes[i, j] = type;
                count++;
            }
        }
        return terrainTypes;
    }

    public uint[] DeserializeActiveIDs( MapSave save ) {
        uint[] ids = new uint[save.gpuData.Length];
        for ( int i = 0; i < ids.Length; i++ ) {
            ids[i] = (uint)save.gpuData[i].y;
        }
        return ids;
    }
}
