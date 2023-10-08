using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DV.CashRegister;
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
        private static readonly Regex VANILLA_STREAMER_SCENE_PATTERN = new Regex("Far__x[0-9]+_z[0-9]+");

        public static Action OnCleanup;

        private static bool isMapLoaded;
        private static List<AssetBundle> loadedAssetBundles;
        private static string originalRailwayScenePath;
        private static string originalGameContentScenePath;
        private static int scenesToLoad;
        //todo how do i make requestedBundle an argument to LoadAssetBundle? "Iterators cannot have 'out' parameters"
        private static AssetBundle requestedBundle;
        private static MapInfo mapInfo;

        public static IEnumerator LoadMap(BasicMapInfo basicMapInfo)
        {
            Mapify.LogDebug(() => $"Loading map {basicMapInfo.name}");

            if (isMapLoaded)
                throw new InvalidOperationException("Map is already loaded");
            isMapLoaded = true;

            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            DisplayLoadingInfo loadingInfo = Object.FindObjectOfType<DisplayLoadingInfo>();

            string loadingMapLogMsg = Locale.Get(Locale.LOADING__LOADING_MAP, basicMapInfo.name);
            loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, 0);
            yield return null;

            // Load asset bundles
            string assetsDir = Maps.GetDirectory(basicMapInfo);
            loadedAssetBundles = new List<AssetBundle>();

            yield return LoadCustomMapAssets(assetsDir, loadingInfo, loadingMapLogMsg);
            yield return LoadAssetBundle(Names.SCENES_ASSET_BUNDLE, assetsDir, loadingInfo, loadingMapLogMsg);
            yield return LoadMapInfo(assetsDir, loadingInfo, loadingMapLogMsg);
            yield return LoadMiscAssets(assetsDir, loadingInfo, loadingMapLogMsg);

            // Load scenes for us to steal assets from
            MonoBehaviourDisablerPatch.DisableAll();

            // Register scene loaded hook
            SceneManager.sceneLoaded += OnSceneLoad;

            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = "vanilla assets";
            string[] names = wsi.transform.FindChildByName("[far]").GetComponent<Streamer>().sceneCollection.names;
            scenesToLoad = names.Length + 2; // Railway and GameContent
            int totalScenesToLoad = scenesToLoad;
            foreach (string name in names)
                SceneManager.LoadSceneAsync(name.Replace(".unity", ""), LoadSceneMode.Additive);
            foreach (Streamer s in wsi.GetComponentsInChildren<Streamer>())
                Object.Destroy(s);

            originalRailwayScenePath = wsi.railwayScenePath;
            originalGameContentScenePath = wsi.gameContentScenePath;

            // Load our scenes, not the vanilla ones
            wsi.terrainsScenePath = Scenes.TERRAIN;
            wsi.railwayScenePath = Scenes.RAILWAY;
            wsi.gameContentScenePath = Scenes.GAME_CONTENT;

            SceneManager.LoadSceneAsync(originalRailwayScenePath, LoadSceneMode.Additive);
            SceneManager.LoadSceneAsync(originalGameContentScenePath, LoadSceneMode.Additive);

            while (scenesToLoad > 0)
            {
                loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, Mathf.RoundToInt((totalScenesToLoad - (float)scenesToLoad) / totalScenesToLoad * 100));
                yield return null;
            }

            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = null;

            Mapify.Log("Vanilla scenes unloaded");
            MonoBehaviourDisablerPatch.EnableAll();

            SetLevelInfo();
            SetupStreamer(wsi.gameObject, mapInfo);

            InitializeLists();
            WorldStreamingInit_Awake_Patch.CanInitialize = true;

            foreach (VanillaAsset nonInstantiatableAsset in Enum.GetValues(typeof(VanillaAsset)).Cast<VanillaAsset>().Where(e => !AssetCopier.InstantiatableAssets.Contains(e)))
                Mapify.LogError($"VanillaAsset {nonInstantiatableAsset} wasn't set in the AssetCopier! You MUST fix this!");
        }

        private static void SetLevelInfo()
        {
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
        }

        private static IEnumerator LoadAssetBundle(string bundlePath, string assetsDir, DisplayLoadingInfo loadingInfo, string loadingMapLogMsg)
        {
            Mapify.LogDebug(() => $"Loading AssetBundle '{bundlePath}'");
            var bundleRequest = AssetBundle.LoadFromFileAsync(Maps.GetMapAsset(bundlePath, assetsDir));
            DisplayLoadingInfo_OnLoadingStatusChanged_Patch.what = bundlePath;
            do
            {
                loadingInfo.UpdateLoadingStatus(loadingMapLogMsg, Mathf.RoundToInt(bundleRequest.progress * 100));
                yield return null;
            } while (!bundleRequest.isDone);

            loadedAssetBundles.Add(bundleRequest.assetBundle);
            requestedBundle = bundleRequest.assetBundle;
        }

        private static IEnumerator LoadMapInfo(string assetsDir, DisplayLoadingInfo loadingInfo, string loadingMapLogMsg)
        {
            yield return LoadAssetBundle(Names.MAP_INFO_ASSET_BUNDLE, assetsDir, loadingInfo, loadingMapLogMsg);

            mapInfo = requestedBundle.LoadAllAssets<MapInfo>()[0];
            if (mapInfo is null)
            {
                Debug.LogError("Failed to load mapinfo");
            }
            Maps.RegisterLoadedMap(mapInfo);
        }

        private static IEnumerator LoadMiscAssets(string assetsDir, DisplayLoadingInfo loadingInfo, string loadingMapLogMsg)
        {
            string[] bundlePaths = Maps.GetMapAssets(Names.ASSETS_ASSET_BUNDLES_PREFIX+"*", assetsDir);

            foreach (var aPath in bundlePaths)
            {
                var bundleFileName = Path.GetFileName(aPath);
                // .manifest files are not asset bundles
                if (bundleFileName.EndsWith(".manifest")) { continue; }

                yield return LoadAssetBundle(bundleFileName, assetsDir, loadingInfo, loadingMapLogMsg);
            }
        }

        private static IEnumerator LoadCustomMapAssets(string assetsDir, DisplayLoadingInfo loadingInfo, string loadingMapLogMsg)
        {
            var bundlePath = Names.CUSTOM_MAP_ASSETS_ASSET_BUNDLE;
            var customStuffBundlePath = Path.Combine(assetsDir, bundlePath);
            if (File.Exists(customStuffBundlePath))
            {
                yield return LoadAssetBundle(bundlePath, assetsDir, loadingInfo, loadingMapLogMsg);
                StartLoadingScreenMusic(requestedBundle);
                yield return null;
                ShowLoadingScreenImage(requestedBundle);
            }
        }

        private static void ShowLoadingScreenImage(AssetBundle bundle)
        {
            Mapify.Log("Checking for custom loading screen images");

            Texture2D customImage = null;

            // bundle.LoadAsset doesnt work for some reason
            // var customImage = bundle.LoadAsset<Texture2D>(Names.CUSTOM_MAP_ASSSETS_PATH + "loading_screen_image.jpg");
            foreach (var ass in bundle.LoadAllAssets())
            {
                if (ass.name == "loading_screen_image")
                {
                    customImage = (Texture2D)ass;
                    break;
                }
            }

            if (customImage is null)
            {
                Mapify.Log("No custom loading screen image found");
                return;
            }

            Mapify.Log("Showing custom loading screen image");

            GameObject canvasGameObject = null;
            foreach (GameObject go in Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (go.name.Contains("LoadImage_Background_"))
                {
                    canvasGameObject = go;
                    break;
                }
            }

            if (canvasGameObject is null)
            {
                Mapify.LogError("cant find canvasGameObject");
                return;
            }

            // set the image
            canvasGameObject.GetComponent<CanvasRenderer>().SetTexture(customImage);
        }

        private static void StartLoadingScreenMusic(AssetBundle bundle)
        {
            Mapify.Log("Checking for custom loading screen music");

            var customMusic = bundle.LoadAsset<AudioClip>(Names.CUSTOM_MAP_ASSSETS_PATH + "loading_screen_music.mp3");
            if (customMusic is null)
            {
                Mapify.Log("No custom loading screen music found");
                return;
            }

            Mapify.Log("Playing custom loading screen music");
            AudioSource mainMenuMusicSource = GameObject.Find("Audio Source - main menu music").GetComponent<AudioSource>();
            mainMenuMusicSource.Pause();
            mainMenuMusicSource.clip = customMusic;
            mainMenuMusicSource.Play();
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
            if (scene.buildIndex == (int)DVScenes.MainMenu)
            {
                Cleanup();
                return;
            }

            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            if (wsi == null) return;
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
                scenesToLoad--;
            }
            else if (scene.path == originalGameContentScenePath)
            {
                Mapify.Log($"Loaded vanilla game content scene at {originalGameContentScenePath}");
                new GameContentCopier().CopyAssets(scene);
                scenesToLoad--;
            }
            else if (VANILLA_STREAMER_SCENE_PATTERN.IsMatch(scene.name))
            {
                new StreamerCopier().CopyAssets(scene);
                scenesToLoad--;
            }
        }

        private static void InitializeLists()
        {
            StationController.allStations = new List<StationController>();
            CashRegisterBase.allCashRegisters = new List<CashRegisterBase>();
        }

        private static void Cleanup()
        {
            OnCleanup();
            Maps.UnregisterLoadedMap();
            SceneManager.sceneLoaded -= OnSceneLoad;
            WorldStreamingInit_Awake_Patch.CanInitialize = false;
            AssetCopier.Cleanup();
            originalRailwayScenePath = null;
            originalGameContentScenePath = null;
            scenesToLoad = 0;

            UnloadAndUnset(ref loadedAssetBundles);

            isMapLoaded = false;
        }

        private static void UnloadAndUnset(ref List<AssetBundle> bundles)
        {
            for (var i = 0; i < bundles.Count; i++)
            {
                if (bundles[i] is null) continue;

                bundles[i].Unload(true);
            }

            bundles = new List<AssetBundle>();
        }
    }
}
