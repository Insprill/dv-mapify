using System;
using System.Collections;
using System.Linq;
using Mapify.Components;
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
            string loadingMapLogMsg = $"loading map {Main.Settings.MapName}";
            wsi.Log(loadingMapLogMsg, 0);
            yield return null;

            // Load asset bundles
            AssetBundleCreateRequest assetsReq = AssetBundle.LoadFromFileAsync(Main.GetLoadedMapAssetPath("assets"));
            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = "assets";
            do
            {
                wsi.Log(loadingMapLogMsg, Mathf.RoundToInt(assetsReq.progress * 100));
                yield return null;
            } while (!assetsReq.isDone);

            assets = assetsReq.assetBundle;

            AssetBundleCreateRequest scenesReq = AssetBundle.LoadFromFileAsync(Main.GetLoadedMapAssetPath("scenes"));
            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = "scenes";
            do
            {
                wsi.Log(loadingMapLogMsg, Mathf.RoundToInt(scenesReq.progress * 100));
                yield return null;
            } while (!scenesReq.isDone);

            scenes = scenesReq.assetBundle;


            // Load scenes for us to steal assets from
            MonoBehaviourPatch.DisableAll();

            SceneManager.sceneLoaded += OnStreamerSceneLoaded;
            Streamer streamer = wsi.transform.FindChildByName("[far]").GetComponent<Streamer>();
            string[] names = streamer.sceneCollection.names;
            scenesToLoad = names.Length;
            int totalScenesToLoad = scenesToLoad;
            foreach (string name in names)
                SceneManager.LoadSceneAsync(name.Replace(".unity", ""), LoadSceneMode.Additive);

            foreach (Streamer s in wsi.GetComponentsInChildren<Streamer>())
                Object.Destroy(s);

            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = "vanilla assets";
            while (scenesToLoad > 0)
            {
                wsi.Log(loadingMapLogMsg, Mathf.RoundToInt((totalScenesToLoad - (float)scenesToLoad) / totalScenesToLoad * 100));
                yield return null;
            }

            SceneManager.sceneLoaded -= OnStreamerSceneLoaded;
            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = null;

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
            MapInfo mapInfo = Main.LoadedMap = assets.LoadAllAssets<MapInfo>()[0];
            levelInfo.waterLevel = mapInfo.waterLevel;
            levelInfo.worldSize = mapInfo.worldSize;
            levelInfo.worldOffset = Vector3.zero;
            levelInfo.defaultSpawnPosition = mapInfo.defaultSpawnPosition;
            levelInfo.defaultSpawnRotation = mapInfo.defaultSpawnRotation;

            SetupStreamer(wsi.gameObject, mapInfo);

            // Register scene loaded hook
            SceneManager.sceneLoaded += OnSceneLoad;

            WorldStreamingInit.LoadingFinished += OnLoadingFinished;
        }

        private static void OnStreamerSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            scenesToLoad--;
            VanillaStreamerSceneInitializer.SceneLoaded(scene);
        }

        private static void SetupStreamer(GameObject parent, MapInfo mapInfo)
        {
            GameObject streamerObj = parent.NewChild("Streamer");
            streamerObj.tag = Streamer.STREAMERTAG;
            streamerObj.SetActive(false);

            SceneCollection collection = streamerObj.AddComponent<SceneCollection>();
            JsonUtility.FromJsonOverwrite(mapInfo.sceneSplitData, collection);
            if (collection.names == null || collection.names.Length == 0)
            {
                // A streamer with no scenes will mark all positions as unloaded, and the game will get stuck on the loading screen.
                Main.Log("No streamer scenes found, destroying!");
                Object.Destroy(streamerObj);
                return;
            }


            Streamer streamer = streamerObj.AddComponent<Streamer>();
            streamer.streamerActive = false;
            ushort size = mapInfo.worldLoadingRingSize;
            streamer.loadingRange = new Vector3(size, 0, size);
            streamer.deloadingRange = new Vector3(size, 0, size);
            streamer.destroyTileDelay = 1.3f;
            streamer.sceneLoadWaitFrames = 1;
            streamer.sceneCollection = collection;
            streamerObj.SetActive(true);
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            if (scene.path == wsi.terrainsScenePath)
            {
                Main.Log($"Loaded terrain scene at {wsi.terrainsScenePath}");
                TerrainSceneInitializer.SceneLoaded(scene);
            }
            else if (scene.path == wsi.railwayScenePath)
            {
                Main.Log($"Loaded railway scene at {wsi.railwayScenePath}");
                RailwaySceneInitializer.SceneLoaded(scene);
            }
            else if (scene.path == wsi.gameContentScenePath)
            {
                Main.Log($"Loaded game content scene at {wsi.gameContentScenePath}");
                GameContentSceneInitializer.SceneLoaded(scene);
            }
            else if (scene.path == originalRailwayScenePath)
            {
                Main.Log($"Loaded vanilla railway scene at {originalRailwayScenePath}");
                VanillaRailwaySceneInitializer.SceneLoaded(scene);
            }
            else if (scene.path == originalGameContentScenePath)
            {
                Main.Log($"Loaded vanilla game content scene at {originalGameContentScenePath}");
                VanillaGameContentSceneInitializer.SceneLoaded(scene);
                MonoBehaviourPatch.EnableAllLater();
                WorldStreamingInit_Awake_Patch.CanInitialize = true;
                foreach (VanillaAsset nonInstantiatableAsset in Enum.GetValues(typeof(VanillaAsset)).Cast<VanillaAsset>().Where(e => !AssetCopier.InstantiatableAssets.Contains(e)))
                    Main.LogError($"VanillaAsset {nonInstantiatableAsset} wasn't set in the AssetCopier! You MUST fix this!");
            }
        }

        private static void OnLoadingFinished()
        {
            if (!Main.LoadedMap.allowTrackBuilding)
                return;
            GameObject playerGO = PlayerManager.PlayerTransform.gameObject;
            playerGO.AddComponent<RailwayBuilder>();
        }
    }
}
