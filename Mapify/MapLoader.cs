using System.Collections;
using System.IO;
using Mapify.Editor;
using Mapify.Patches;
using Mapify.SceneInitializers;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify
{
    public static class MapLoader
    {
        private static string originalRailwayScenePath;
        private static string originalGameContentScenePath;
        private static AssetBundle assets;
        private static AssetBundle scenes;

        public static IEnumerator LoadMap()
        {
            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            wsi.Log($"loading map {Main.Settings.MapName}", 0);
            yield return null;

            // Load asset bundles
            string mapDir = Main.Settings.MapDir;
            assets = AssetBundle.LoadFromFile(Path.Combine(mapDir, "assets"));
            scenes = AssetBundle.LoadFromFile(Path.Combine(mapDir, "scenes"));

            // Load scenes for us to steal assets from
            wsi.Log("copying vanilla assets", 12);
            yield return null;
            MonoBehaviourPatch.DisableAll();
            originalRailwayScenePath = wsi.railwayScenePath;
            SceneManager.LoadScene(originalRailwayScenePath, LoadSceneMode.Additive);
            originalGameContentScenePath = wsi.gameContentScenePath;
            SceneManager.LoadScene(originalGameContentScenePath, LoadSceneMode.Additive);

            // todo: do we need to hardcode these?
            // Load our scenes, not the vanilla ones
            wsi.terrainsScenePath = "Assets/Scenes/Terrain.unity";
            wsi.railwayScenePath = "Assets/Scenes/Railway.unity";
            wsi.gameContentScenePath = "Assets/Scenes/GameContent.unity";

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

            // Register loading finished hook to cleanup
            WorldStreamingInit.LoadingFinished += LoadingFinished;

            // Destroy world streamers
            foreach (Streamer streamer in Object.FindObjectsOfType<Streamer>())
                Object.Destroy(streamer);
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
                GameContentSceneInitializer.SceneLoaded(scene);
            }
            else if (scene.path == originalRailwayScenePath)
            {
                Main.Logger.Log($"Loaded vanilla railway scene at {originalRailwayScenePath}");
                VanillaRailwaySceneInitializer.SceneLoaded(scene);
            }
            else if (scene.path == originalGameContentScenePath)
            {
                Main.Logger.Log($"Loaded vanilla game content scene at {originalGameContentScenePath}");
                VanillaGameContentSceneInitializer.SceneLoaded(scene);
                MonoBehaviourPatch.EnableAllLater();
                WorldStreamingInit_Awake_Patch.CanInitialize = true;
            }
        }

        private static void LoadingFinished()
        {
            Main.Logger.Log("Cleaning up unused assets");
            assets.Unload(false);
            scenes.Unload(false);
        }
    }
}
