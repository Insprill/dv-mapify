using DV.TerrainSystem;
using DV.Utils;
using DV.WorldTools;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers
{
    public static class TerrainSceneInitializer
    {
        public static void SceneLoaded(Scene scene)
        {
            SetupTerrainGrid();
            SetupDistantTerrain();
        }

        private static void SetupTerrainGrid()
        {
            GameObject gridObject = WorldMover.Instance.NewChildWithPosition("TerrainGrid", new Vector3(0, Mapify.LoadedMap.terrainHeight, 0));
            gridObject.SetActive(false);
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.loadingRingSize = Mapify.LoadedMap.terrainLoadingRingSize;
            grid.addToVegetationStudio = false;
            grid.pixelError = Mapify.LoadedMap.terrainPixelError;
            grid.drawInstanced = Mapify.LoadedMap.terrainDrawInstanced;
            grid.vegetationReloadWaitFrames = 2;
            grid.maxConcurrentLoads = 3;
            Layer.Terrain.Apply(grid);
            TerrainGrid.Initialized += () => OnTerrainInitialized(grid);
            gridObject.SetActive(true);
        }

        private static void SetupDistantTerrain()
        {
            GameObject distantTerrainObject = new GameObject("[distant terrain]");
            distantTerrainObject.transform.SetParent(WorldMover.Instance.originShiftParent);

            DistantTerrain distantTerrain = distantTerrainObject.gameObject.AddComponent<DistantTerrain>();
            distantTerrain.worldScale = SingletonBehaviour<LevelInfo>.Instance.worldSize;
            distantTerrain.step = 128; // No idea what this means but this is what it's set to in the game.
        }

        private static void OnTerrainInitialized(TerrainGrid grid)
        {
            foreach (GameObject obj in grid.generatedTerrains)
            {
                Terrain terrain = obj.GetComponent<Terrain>();
                terrain.materialTemplate = Mapify.LoadedMap.terrainMaterial;
                terrain.basemapDistance = Mapify.LoadedMap.terrainBasemapDistance;
            }
        }
    }
}
