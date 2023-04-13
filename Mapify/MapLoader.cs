using System;
using System.Collections;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Patches;
using Mapify.SceneInitializers;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify
{
    public static class MapLoader
    {
        private static string originalRailwayScenePath;
        private static string originalGameContentScenePath;
        private static AssetBundle assets;
        private static AssetBundle scenes;

        private static int scenesToLoad;

        public static IEnumerator LoadMap()
        {
            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            wsi.Log($"loading map {Main.Settings.MapName}", 0);
            yield return null;

            // Load asset bundles
            assets = AssetBundle.LoadFromFile(Main.GetLoadedMapAssetPath("assets"));
            scenes = AssetBundle.LoadFromFile(Main.GetLoadedMapAssetPath("scenes"));

            // Load scenes for us to steal assets from
            wsi.Log("copying vanilla assets", 12);
            yield return null;
            MonoBehaviourPatch.DisableAll();

            SceneManager.sceneLoaded += OnStreamerSceneLoaded;
            Streamer streamer = wsi.transform.FindChildByName("[far]").GetComponent<Streamer>();
            string[] names = streamer.sceneCollection.names;
            scenesToLoad = names.Length;
            foreach (string name in names)
                SceneManager.LoadSceneAsync(name.Replace(".unity", ""), LoadSceneMode.Additive);

            foreach (Streamer s in wsi.GetComponentsInChildren<Streamer>())
                Object.Destroy(s);

            while (scenesToLoad > 0)
                yield return null;
            SceneManager.sceneLoaded -= OnStreamerSceneLoaded;

            originalRailwayScenePath = wsi.railwayScenePath;
            SceneManager.LoadScene(originalRailwayScenePath, LoadSceneMode.Additive);
            originalGameContentScenePath = wsi.gameContentScenePath;
            SceneManager.LoadScene(originalGameContentScenePath, LoadSceneMode.Additive);

            // Load our scenes, not the vanilla ones
            wsi.terrainsScenePath = Scenes.TERRAIN;
            wsi.railwayScenePath = Scenes.RAILWAY;
            wsi.gameContentScenePath = Scenes.GAME_CONTENT;

            // Set LevelInfo
            LevelInfo levelInfo = SingletonBehaviour<LevelInfo>.Instance;
            Main.LoadedMap = assets.LoadAllAssets<MapInfo>()[0];
            levelInfo.waterLevel = Main.LoadedMap.waterLevel;
            levelInfo.worldSize = Main.LoadedMap.worldSize;
            levelInfo.worldOffset = Vector3.zero;
            levelInfo.defaultSpawnPosition = Main.LoadedMap.defaultSpawnPosition;
            levelInfo.defaultSpawnRotation = Main.LoadedMap.defaultSpawnRotation;

            // Register scene loaded hook
            SceneManager.sceneLoaded += OnSceneLoad;

            // Register loading finished hook to cleanup
            WorldStreamingInit.LoadingFinished += LoadingFinished;
        }

        private static void OnStreamerSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            scenesToLoad--;
            VanillaStreamerSceneInitializer.SceneLoaded(scene);
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
                foreach (VanillaAsset nonInstantiatableAsset in Enum.GetValues(typeof(VanillaAsset)).Cast<VanillaAsset>().Where(e => !AssetCopier.InstantiatableAssets.Contains(e)))
                    Main.Logger.Error($"VanillaAsset {nonInstantiatableAsset} wasn't set in the AssetCopier! You MUST fix this!");
            }
        }


        private static void LoadingFinished()
        {
            Main.Logger.Log("Destroying streamers");
            foreach (Streamer streamer in Object.FindObjectsOfType<Streamer>())
                Object.Destroy(streamer);
            Main.Logger.Log("Cleaning up unused assets");
            assets.Unload(false);
            scenes.Unload(false);
        }
    }
}
