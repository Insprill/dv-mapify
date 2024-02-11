using DV.TerrainSystem;
using DV.Utils;
using Mapify.Map;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.Terrain
{
    [SceneSetupPriority(int.MinValue)]
    public class TerrainGridSetup : SceneSetup
    {
        public override void Run()
        {
            GameObject gridObject = WorldMover.OriginShiftParent.gameObject.NewChildWithPosition("TerrainGrid", new Vector3(0, Maps.LoadedMap.terrainHeight, 0));
            gridObject.SetActive(false);
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.loadingRingSize = Maps.LoadedMap.terrainLoadingRingSize;
            grid.addToVegetationStudio = false;
            grid.pixelError = Maps.LoadedMap.terrainPixelError;
            grid.drawInstanced = Maps.LoadedMap.terrainDrawInstanced;
            grid.vegetationReloadWaitFrames = 2;
            grid.maxConcurrentLoads = 3;
            Layer.Terrain.Apply(grid);
            TerrainGrid.Initialized += OnTerrainInitialized;
            gridObject.SetActive(true);
        }

        private static void OnTerrainInitialized()
        {
            TerrainGrid.Initialized -= OnTerrainInitialized;
            foreach (GameObject obj in SingletonBehaviour<TerrainGrid>.Instance.generatedTerrains)
            {
                UnityEngine.Terrain terrain = obj.GetComponent<UnityEngine.Terrain>();
                terrain.materialTemplate = Maps.LoadedMap.terrainMaterial;
                terrain.basemapDistance = Maps.LoadedMap.terrainBasemapDistance;
            }
        }
    }
}
