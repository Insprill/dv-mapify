using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using DV;
using DV.RenderTextureSystem;
using DV.TerrainSystem;
using DV.WorldTools;
using HarmonyLib;
using Mapify.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldStreamingInit), "Awake")]
    public static class WorldStreamingInit_Awake_Patch
    {
        public static bool Prefix(WorldStreamingInit __instance)
        {
            // todo: can we do this without the user specifying the bundles and us hardcoding the names?
            // Load asset bundles
            AssetBundle assets = AssetBundle.LoadFromFile(Path.Combine(Main.ModEntry.Path, "Map/assets"));
            AssetBundle.LoadFromFile(Path.Combine(Main.ModEntry.Path, "Map/scenes"));

            // todo: do we need to hardcode these?
            // Set what scenes to load
            __instance.terrainsScenePath = "Assets/Scenes/Terrain.unity";
            __instance.railwayScenePath = "Assets/Scenes/Railway.unity";
            __instance.gameContentScenePath = "Assets/Scenes/GameContent.unity";

            // Set LevelInfo
            MapInfo mapInfo = assets.LoadAllAssets<MapInfo>()[0];
            LevelInfo levelInfo = SingletonBehaviour<LevelInfo>.Instance;
            levelInfo.waterLevel = mapInfo.waterLevel;
            levelInfo.worldSize = mapInfo.worldSize;
            levelInfo.worldOffset = mapInfo.worldOffset;
            levelInfo.defaultSpawnPosition = mapInfo.defaultSpawnPosition;
            levelInfo.defaultSpawnRotation = mapInfo.defaultSpawnRotation;

            // Register scene loaded hook
            SceneManager.sceneLoaded += OnSceneLoad;

            foreach (Streamer streamer in Object.FindObjectsOfType<Streamer>())
            {
                Object.Destroy(streamer);
            }
            return true;
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            if (scene.path == wsi.terrainsScenePath)
            {
                Main.Logger.Log($"Loading terrain scene at {wsi.terrainsScenePath}");
                SetupDistantTerrain(scene.GetRootGameObjects().FirstOrDefault(o => o.name == "[distant terrain]"));
                SetupTerrainGrid();
            }
            else if (scene.path == wsi.railwayScenePath)
            {
                Main.Logger.Log($"Loading railway scene at {wsi.railwayScenePath}");
                SetupRailTracks();
            }
            else if (scene.path == wsi.gameContentScenePath)
            {
                Main.Logger.Log($"Loading game content scene at {wsi.gameContentScenePath}");
                SetupGameScene();
            }
        }

        private static void SetupDistantTerrain(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Main.Logger.Error("Failed to find [distant terrain]!");
                return;
            }

            DistantTerrain distantTerrain = gameObject.gameObject.AddComponent<DistantTerrain>();
            distantTerrain.worldScale = SingletonBehaviour<LevelInfo>.Instance.worldSize;
            distantTerrain.step = 128; // No idea what this means but this is what it's set to in the game.
        }

        private static void SetupTerrainGrid()
        {
            GameObject gridObject = new GameObject();
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.loadingRingSize = 2;
            grid.addToVegetationStudio = false;
            grid.pixelError = 10;
            grid.drawInstanced = true;
            grid.terrainLayer = 8;
            grid.vegetationReloadWaitFrames = 2;
            grid.maxConcurrentLoads = 3;
        }

        private static void SetupRailTracks()
        {
            foreach (Track track in Object.FindObjectsOfType<Track>())
            {
                BezierCurve curve = track.gameObject.GetComponent<BezierCurve>();
                curve.resolution = 0.5f;
                RailTrack railTrack = track.gameObject.AddComponent<RailTrack>();
                if (!railTrack.CurveIsValid())
                {
                    Main.Logger.Warning($"Curve on track {track.name} isn't valid!");
                    continue;
                }

                railTrack.age = track.age;
                railTrack.ApplyRailType();
                if (railTrack.generateColliders)
                    railTrack.CreateCollider();
            }
        }

        // todo: is there a nicer way we can do this? maybe create a prefab for it?
        private static void SetupGameScene()
        {
            Main.Logger.Log("Creating SaveLoadController");
            GameObject licensesAndGarages = new GameObject("[LicensesAndGarages]");
            licensesAndGarages.AddComponent<SaveLoadController>();
            Main.Logger.Log("Creating CarSpawner");
            GameObject carSpawner = new GameObject("[CarSpawner]");
            carSpawner.AddComponent<CarSpawner>();
            carSpawner.AddComponent<CarSpawnerOriginShiftHandler>();
            Main.Logger.Log("Creating ItemDisablerGrid");
            GameObject jobLogicController = new GameObject("[JobLogicController]");
            jobLogicController.AddComponent<LogicController>();
            Main.Logger.Log("Creating ItemDisablerGrid");
            GameObject derailAndDamageObserver = new GameObject("[DerailAndDamageObserver]");
            derailAndDamageObserver.AddComponent<DerailAndDamageObserver>();
            Main.Logger.Log("Creating StorageLogic");
            GameObject storageLogic = new GameObject("[StorageLogic]");
            GameObject storageWorld = new GameObject("StorageWorld") {
                transform = {
                    parent = storageLogic.transform
                }
            };
            storageWorld.AddComponent<StorageBase>().storageType = StorageType.World;
            GameObject storageLostAndFound = new GameObject("StorageLostAndFound") {
                transform = {
                    parent = storageLogic.transform
                }
            };
            storageLostAndFound.AddComponent<StorageBase>().storageType = StorageType.LostAndFound;
            GameObject storageInventory = new GameObject("StorageInventory") {
                transform = {
                    parent = storageLogic.transform
                }
            };
            storageInventory.AddComponent<StorageBase>().storageType = StorageType.Inventory;
            GameObject storageBelt = new GameObject("StorageBelt") {
                transform = {
                    parent = storageLogic.transform
                }
            };
            storageBelt.AddComponent<StorageBase>().storageType = StorageType.Belt;
            storageLogic.AddComponent<StorageController>(); // Must be added after all StorageBase's
            Main.Logger.Log("Creating ItemDisablerGrid");
            GameObject itemDisablerGrid = new GameObject("[ItemDisablerGrid]");
            itemDisablerGrid.AddComponent<ItemDisablerGrid>();
            Main.Logger.Log("Creating ShopLogic");
            GameObject shopLogic = new GameObject("[ShopLogic]");
            GlobalShopController globalShopController = shopLogic.AddComponent<GlobalShopController>();
            // todo: add support for shops
            globalShopController.globalShopList = new List<Shop>();
            globalShopController.shopItemsData = new List<ShopItemData>();
            Main.Logger.Log("Creating RenderTextureSystem");
            GameObject renderTextureSystem = new GameObject("[RenderTextureSystem]");
            renderTextureSystem.AddComponent<RenderTextureSystem>();
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
                if (codes[i].opcode == OpCodes.Ldstr && (codes[i].operand as string) == "loading terrain")
                {
                    codes.RemoveRange(i - 9, 8);
                    break;
                }

            return codes.AsEnumerable();
        }
    }
}
