using DV.TerrainSystem;
using DV.Utils;
using DV.WorldTools;
using Mapify.Map;
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
            SetupTerrainHoleFinder();
            SetupDistantTerrain();
            SetupSingletonInstanceFinder();
        }

        private static void SetupTerrainHoleFinder()
        {
            TerrainHoleManager terrainHoleManager = new GameObject(nameof(TerrainHoleManager)).AddComponent<TerrainHoleManager>();
            terrainHoleManager.maxHoles = 100; // Todo: update when tunnels are a thing
        }

        private static void SetupTerrainGrid()
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

        private static void SetupDistantTerrain()
        {
            GameObject distantTerrainObject = new GameObject("[distant terrain]");
            distantTerrainObject.transform.SetParent(WorldMover.Instance.originShiftParent);

            LevelInfo levelInfo = SingletonBehaviour<LevelInfo>.Instance;
            DistantTerrain distantTerrain = distantTerrainObject.gameObject.AddComponent<DistantTerrain>();
            distantTerrain.trackingReference = SingletonBehaviour<WorldMover>.Instance.playerTracker.GetTrackerTransform();
            distantTerrain.singleTerrainSize = levelInfo.terrainSize;
            distantTerrain.worldScale = levelInfo.worldSize;
            distantTerrain.step = 128; // No idea what this means but this is what it's set to in the game.
        }

        private static void OnTerrainInitialized(TerrainGrid grid)
        {
            foreach (GameObject obj in grid.generatedTerrains)
            {
                Terrain terrain = obj.GetComponent<Terrain>();
                terrain.materialTemplate = Maps.LoadedMap.terrainMaterial;
                terrain.basemapDistance = Maps.LoadedMap.terrainBasemapDistance;
            }
        }

        private static void SetupSingletonInstanceFinder()
        {
            new GameObject(nameof(SingletonInstanceFinder)).AddComponent<SingletonInstanceFinder>();
        }
    }
}
