using System;
using System.Collections;
using System.Linq;
using DV.Utils;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Patches;
using Mapify.SceneInitializers.GameContent;
using Mapify.SceneInitializers.Railway;
using Mapify.SceneInitializers.Terrain;
using Mapify.SceneInitializers.Vanilla.GameContent;
using Mapify.SceneInitializers.Vanilla.Railway;
using Mapify.SceneInitializers.Vanilla.Streaming;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify.Map
{
    public static class MapLifeCycle
    {
        private static bool isMapLoaded;

        private static AssetBundle assets;
        private static AssetBundle scenes;
        private static string originalRailwayScenePath;
        private static string originalGameContentScenePath;
        private static int scenesToLoad;

        public static IEnumerator LoadMap(BasicMapInfo basicMapInfo)
        {
            Mapify.LogDebug($"Loading map {basicMapInfo.mapName}");

            if (isMapLoaded)
                throw new InvalidOperationException("Map is already loaded");
            isMapLoaded = true;

            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            DisplayLoadingInfo loadingInfo = Object.FindObjectOfType<DisplayLoadingInfo>();

            string loadingMapLogMsg = Locale.Get(Locale.LOADING__LOADING_MAP, basicMapInfo.mapName);
            loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, 0);
            yield return null;

            // Load asset bundles
            Mapify.LogDebug($"Loading AssetBundle '{Names.ASSETS_ASSET_BUNDLE}'");
            string mapDir = Maps.GetDirectory(basicMapInfo);
            AssetBundleCreateRequest assetsReq = AssetBundle.LoadFromFileAsync(Maps.GetMapAsset(Names.ASSETS_ASSET_BUNDLE, mapDir));
            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = Names.ASSETS_ASSET_BUNDLE;
            do
            {
                loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, Mathf.RoundToInt(assetsReq.progress * 100));
                yield return null;
            } while (!assetsReq.isDone);

            assets = assetsReq.assetBundle;

            Mapify.LogDebug($"Loading AssetBundle '{Names.SCENES_ASSET_BUNDLE}'");
            AssetBundleCreateRequest scenesReq = AssetBundle.LoadFromFileAsync(Maps.GetMapAsset(Names.SCENES_ASSET_BUNDLE, mapDir));
            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = Names.SCENES_ASSET_BUNDLE;
            do
            {
                loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, Mathf.RoundToInt(scenesReq.progress * 100));
                yield return null;
            } while (!scenesReq.isDone);

            scenes = scenesReq.assetBundle;

            // Register MapInfo
            MapInfo mapInfo = assets.LoadAllAssets<MapInfo>()[0];
            Maps.RegisterLoadedMap(mapInfo);

            // Load scenes for us to steal assets from
            MonoBehaviourDisablerPatch.DisableAll();

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
                loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, Mathf.RoundToInt((totalScenesToLoad - (float)scenesToLoad) / totalScenesToLoad * 100));
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
            levelInfo.terrainSize = mapInfo.terrainSize;
            levelInfo.waterLevel = mapInfo.waterLevel;
            levelInfo.worldSize = mapInfo.worldSize;
            levelInfo.worldOffset = Vector3.zero;
            levelInfo.defaultSpawnPosition = mapInfo.defaultSpawnPosition;
            levelInfo.defaultSpawnRotation = mapInfo.defaultSpawnRotation;
            levelInfo.newCareerSpawnPosition = mapInfo.defaultSpawnPosition;
            levelInfo.newCareerSpawnRotation = mapInfo.defaultSpawnRotation;
            levelInfo.enforceBoundary = true;
            levelInfo.worldBoundaryMargin = mapInfo.worldBoundaryMargin;

            SetupStreamer(wsi.gameObject, mapInfo);

            // Register scene loaded hook
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private static void OnStreamerSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            scenesToLoad--;
            new StreamerCopier().CopyAssets(scene);
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
                Mapify.Log("No streamer scenes found, destroying!");
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
                Mapify.Log($"Loaded terrain scene at {wsi.terrainsScenePath}");
                new TerrainSceneInitializer(scene).Run();
            }
            else if (scene.path == wsi.railwayScenePath)
            {
                Mapify.Log($"Loaded railway scene at {wsi.railwayScenePath}");
                new RailwaySceneInitializer(scene).Run();
            }
            else if (scene.path == wsi.gameContentScenePath)
            {
                Mapify.Log($"Loaded game content scene at {wsi.gameContentScenePath}");
                new GameContentSceneInitializer(scene).Run();
            }
            else if (scene.path == originalRailwayScenePath)
            {
                Mapify.Log($"Loaded vanilla railway scene at {originalRailwayScenePath}");
                new RailwayCopier().CopyAssets(scene);
            }
            else if (scene.path == originalGameContentScenePath)
            {
                Mapify.Log($"Loaded vanilla game content scene at {originalGameContentScenePath}");
                new GameContentCopier().CopyAssets(scene);
                MonoBehaviourDisablerPatch.EnableAllLater();
                WorldStreamingInit_Awake_Patch.CanInitialize = true;
                foreach (VanillaAsset nonInstantiatableAsset in Enum.GetValues(typeof(VanillaAsset)).Cast<VanillaAsset>().Where(e => !AssetCopier.InstantiatableAssets.Contains(e)))
                    Mapify.LogError($"VanillaAsset {nonInstantiatableAsset} wasn't set in the AssetCopier! You MUST fix this!");
            }
            else if (scene.buildIndex == (int)DVScenes.MainMenu)
            {
                Cleanup();
            }
        }

        private static void Cleanup()
        {
            originalRailwayScenePath = null;
            originalGameContentScenePath = null;
            scenesToLoad = 0;
            if (assets != null)
            {
                assets.Unload(true);
                assets = null;
            }

            if (scenes != null)
            {
                scenes.Unload(true);
                scenes = null;
            }

            isMapLoaded = false;
        }
    }
}
