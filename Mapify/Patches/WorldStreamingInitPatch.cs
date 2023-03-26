using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Mapify.Editor;
using Mapify.SceneInitializers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldStreamingInit), "Awake")]
    public static class WorldStreamingInit_Awake_Patch
    {
        private static string originalRailwayScenePath;

        public static bool Prefix(WorldStreamingInit __instance)
        {
            Main.Logger.Log($"Loading map {Main.Settings.MapName}");

            // Load asset bundles
            string mapDir = Main.Settings.MapDir;
            AssetBundle assets = AssetBundle.LoadFromFile(Path.Combine(mapDir, "assets"));
            AssetBundle.LoadFromFile(Path.Combine(mapDir, "scenes"));

            // Load default railway scene for us to steal assets from
            originalRailwayScenePath = __instance.railwayScenePath;
            Main.Logger.Log($"Loading default railway scene ({originalRailwayScenePath}) to copy assets from");
            SceneManager.LoadScene(originalRailwayScenePath, LoadSceneMode.Additive);

            // todo: do we need to hardcode these?
            // Load our scenes, not the vanilla ones
            __instance.terrainsScenePath = "Assets/Scenes/Terrain.unity";
            __instance.railwayScenePath = "Assets/Scenes/Railway.unity";
            __instance.gameContentScenePath = "Assets/Scenes/GameContent.unity";

            // Set LevelInfo
            Main.MapInfo = assets.LoadAllAssets<MapInfo>()[0];
            LevelInfo levelInfo = SingletonBehaviour<LevelInfo>.Instance;
            levelInfo.waterLevel = Main.MapInfo.waterLevel;
            levelInfo.worldSize = Main.MapInfo.worldSize;
            levelInfo.worldOffset = Vector3.zero;
            levelInfo.defaultSpawnPosition = Main.MapInfo.defaultSpawnPosition;
            levelInfo.defaultSpawnRotation = Main.MapInfo.defaultSpawnRotation;

            // Register scene loaded hook
            SceneManager.sceneLoaded += OnSceneLoad;

            // Destroy world streamers
            foreach (Streamer streamer in Object.FindObjectsOfType<Streamer>()) Object.Destroy(streamer);
            return true;
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            if (scene.path == wsi.terrainsScenePath)
            {
                Main.Logger.Log($"Loaded terrain scene at {wsi.terrainsScenePath}");
                TerrainSceneInitializer.SceneLoaded(scene);
            }
            else if (scene.path == wsi.railwayScenePath)
            {
                Main.Logger.Log($"Loaded railway scene at {wsi.railwayScenePath}");
                RailwaySceneInitializer.SceneLoaded();
            }
            else if (scene.path == wsi.gameContentScenePath)
            {
                Main.Logger.Log($"Loaded game content scene at {wsi.gameContentScenePath}");
                GameContentSceneInitializer.SceneLoaded();
            }
            else if (scene.path == originalRailwayScenePath)
            {
                Main.Logger.Log($"Loaded default railway scene at {originalRailwayScenePath}");
                VanillaRailwaySceneInitializer.SceneLoaded(scene);
            }
        }
    }

    [HarmonyPatch(typeof(WorldStreamingInit), "LoadingRoutine", MethodType.Enumerator)]
    public static class WorldStreamingInit_LoadingRoutine_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
                // Don't add the vegetationStudioPrefab
                if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand as string == "loading terrain")
                {
                    codes.RemoveRange(i - 9, 8);
                    break;
                }

            return codes.AsEnumerable();
        }
    }
}
