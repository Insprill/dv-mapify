using DV.TerrainSystem;
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
            GameObject gridObject = WorldMover.Instance.NewChildWithPosition("TerrainGrid", new Vector3(0, Maps.LoadedMap.terrainHeight, 0));
            gridObject.SetActive(false);
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.loadingRingSize = Maps.LoadedMap.terrainLoadingRingSize;
            grid.addToVegetationStudio = false;
            grid.pixelError = Maps.LoadedMap.terrainPixelError;
            grid.drawInstanced = Maps.LoadedMap.terrainDrawInstanced;
            grid.vegetationReloadWaitFrames = 2;
            grid.maxConcurrentLoads = 3;
            Layer.Terrain.Apply(grid);
            TerrainGrid.Initialized += () => OnTerrainInitialized(grid);
            gridObject.SetActive(true);
        }

        private static void OnTerrainInitialized(TerrainGrid grid)
        {
            foreach (GameObject obj in grid.generatedTerrains)
            {
                UnityEngine.Terrain terrain = obj.GetComponent<UnityEngine.Terrain>();
                terrain.materialTemplate = Maps.LoadedMap.terrainMaterial;
                terrain.basemapDistance = Maps.LoadedMap.terrainBasemapDistance;
            }
        }
    }
}
