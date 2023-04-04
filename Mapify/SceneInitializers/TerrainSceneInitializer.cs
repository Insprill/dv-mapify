using System.Linq;
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
            SetupDistantTerrain(scene.GetRootGameObjects().FirstOrDefault(o => o.name == "[distant terrain]"));
        }

        private static void SetupTerrainGrid()
        {
            GameObject gridObject = WorldMover.Instance.originShiftParent.gameObject.NewChild("TerrainGrid");
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.loadingRingSize = 2;
            grid.addToVegetationStudio = false;
            grid.pixelError = Main.MapInfo.terrainPixelError;
            grid.drawInstanced = Main.MapInfo.terrainDrawInstanced;
            grid.terrainLayer = 8;
            grid.vegetationReloadWaitFrames = 2;
            grid.maxConcurrentLoads = 3;
        }

        private static void SetupDistantTerrain(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Main.Logger.Error("Failed to find [distant terrain]!");
                return;
            }

            gameObject.transform.SetParent(WorldMover.Instance.originShiftParent);
            DistantTerrain distantTerrain = gameObject.gameObject.AddComponent<DistantTerrain>();
            distantTerrain.worldScale = SingletonBehaviour<LevelInfo>.Instance.worldSize;
            distantTerrain.step = 128; // No idea what this means but this is what it's set to in the game.
        }
    }
}
