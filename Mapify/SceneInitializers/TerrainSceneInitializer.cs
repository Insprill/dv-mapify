using DV.TerrainSystem;
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
            GameObject gridObject = WorldMover.Instance.NewChildWithPosition("TerrainGrid", new Vector3(0, Main.LoadedMap.terrainHeight, 0));
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.loadingRingSize = 2;
            grid.addToVegetationStudio = false;
            grid.pixelError = Main.LoadedMap.terrainPixelError;
            grid.drawInstanced = Main.LoadedMap.terrainDrawInstanced;
            grid.terrainLayer = 8;
            grid.vegetationReloadWaitFrames = 2;
            grid.maxConcurrentLoads = 3;
        }

        private static void SetupDistantTerrain()
        {
            GameObject distantTerrainObject = new GameObject("[distant terrain]");
            distantTerrainObject.transform.SetParent(WorldMover.Instance.originShiftParent);

            DistantTerrain distantTerrain = distantTerrainObject.gameObject.AddComponent<DistantTerrain>();
            distantTerrain.worldScale = SingletonBehaviour<LevelInfo>.Instance.worldSize;
            distantTerrain.step = 128; // No idea what this means but this is what it's set to in the game.
        }
    }
}
