using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using DV;
using DV.RenderTextureSystem;
using DV.Signs;
using DV.TerrainSystem;
using DV.WorldTools;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(WorldStreamingInit), "Awake")]
    public static class WorldStreamingInit_Awake_Patch
    {
        private static string originalRailwayScenePath;
        private static readonly Dictionary<string, GameObject> switchPrefabs = new Dictionary<string, GameObject>(4);

        public static bool Prefix(WorldStreamingInit __instance)
        {
            // Load asset bundles
            string mapDir = Main.Settings.MapDir;

            Main.Logger.Log($"Loading map {Main.Settings.MapName}");

            AssetBundle assets = AssetBundle.LoadFromFile(Path.Combine(mapDir, "assets"));
            AssetBundle.LoadFromFile(Path.Combine(mapDir, "scenes"));

            originalRailwayScenePath = __instance.railwayScenePath;
            Main.Logger.Log($"Loading default railway scene ({originalRailwayScenePath}) to copy assets from");
            SceneManager.LoadScene(originalRailwayScenePath, LoadSceneMode.Additive);

            // todo: do we need to hardcode these?
            // Set what scenes to load
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

            foreach (Streamer streamer in Object.FindObjectsOfType<Streamer>()) Object.Destroy(streamer);
            return true;
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            WorldStreamingInit wsi = SingletonBehaviour<WorldStreamingInit>.Instance;
            if (scene.path == wsi.terrainsScenePath)
            {
                Main.Logger.Log($"Loaded terrain scene at {wsi.terrainsScenePath}");
                SetupDistantTerrain(scene.GetRootGameObjects().FirstOrDefault(o => o.name == "[distant terrain]"));
                SetupTerrainGrid();
            }
            else if (scene.path == wsi.railwayScenePath)
            {
                Main.Logger.Log($"Loaded railway scene at {wsi.railwayScenePath}");
                SetupRailTracks();
                CreateSigns();
            }
            else if (scene.path == wsi.gameContentScenePath)
            {
                Main.Logger.Log($"Loaded game content scene at {wsi.gameContentScenePath}");
                SetupGameScene();
            }
            else if (scene.path == originalRailwayScenePath)
            {
                Main.Logger.Log($"Loaded default railway scene at {originalRailwayScenePath}");
                CopyDefaultAssets(originalRailwayScenePath);
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
            grid.pixelError = Main.MapInfo.terrainPixelError;
            grid.drawInstanced = Main.MapInfo.terrainDrawInstanced;
            grid.terrainLayer = 8;
            grid.vegetationReloadWaitFrames = 2;
            grid.maxConcurrentLoads = 3;
        }

        private static void SetupRailTracks()
        {
            Main.Logger.Log("Creating RailTracks");
            Track[] tracks = Object.FindObjectsOfType<Track>();
            foreach (Track track in tracks)
            {
                track.gameObject.SetActive(false);
                RailTrack railTrack = track.gameObject.AddComponent<RailTrack>();
                railTrack.dontChange = false;
                railTrack.age = track.age;
                railTrack.ApplyRailType();
            }

            Main.Logger.Log("Creating Junctions");
            Switch[] switches = Object.FindObjectsOfType<Switch>();
            foreach (Switch sw in switches) CreateJunction(sw);

            Main.Logger.Log("Connecting tracks");
            foreach (Track track in tracks)
            {
                ConnectRailTrack(track);
                track.gameObject.SetActive(true);
            }

            foreach (Switch sw in switches) GameObject.DestroyImmediate(sw.gameObject);

            RailManager.AlignAllTrackEnds();
            RailManager.TestConnections();
        }

        private static void CopyDefaultAssets(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.isLoaded)
            {
                Main.Logger.Error($"Default scene {scenePath} isn't loaded!");
                return;
            }

            GameObject[] gameObjects = scene.GetRootGameObjects();
            GameObject railwayRoot = null;
            foreach (GameObject rootObject in gameObjects)
            {
                rootObject.SetActive(false);
                if (rootObject.name != "[railway]") continue;
                railwayRoot = rootObject;
                Main.Logger.Log("Moving [railway] to the active scene");
                SceneManager.MoveGameObjectToScene(rootObject, SceneManager.GetActiveScene());
                break;
            }

            Main.Logger.Log("Unloading default railway scene");
            // cope
#pragma warning disable CS0618
            SceneManager.UnloadScene(scene);
#pragma warning restore CS0618

            if (railwayRoot == null)
            {
                Main.Logger.Error("Failed to find [railway]!");
                return;
            }

            for (int i = 0; i < railwayRoot.transform.childCount; i++)
            {
                GameObject gameObject = railwayRoot.transform.GetChild(i).gameObject;
                string name = gameObject.name;
                switch (name)
                {
                    case "junc-left":
                    case "junc-right":
                    case "junc-left-outer-sign":
                    case "junc-right-outer-sign":
                        if (switchPrefabs.ContainsKey(name) || gameObject.transform.rotation.x != 0.0f || gameObject.transform.rotation.z != 0.0f) continue;
                        Main.Logger.Log($"Found {name}");
                        CleanupSwitch(gameObject);
                        switchPrefabs[name] = gameObject;
                        break;
                }
            }

            GameObject.DestroyImmediate(railwayRoot);
        }

        private static void CleanupSwitch(GameObject gameObject)
        {
            gameObject.transform.SetParent(null);
            gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            foreach (Junction junction in gameObject.GetComponentsInChildren<Junction>()) Object.Destroy(junction);
            foreach (BezierCurve curve in gameObject.GetComponentsInChildren<BezierCurve>()) GameObject.Destroy(curve.gameObject);
            gameObject.SetActive(false);
        }

        private static void CreateJunction(Switch sw)
        {
            Transform swTransform = sw.transform;
            GameObject prefabClone = GameObject.Instantiate(switchPrefabs[sw.SwitchPrefabName]);
            Transform prefabCloneTransform = prefabClone.transform;
            Transform inJunction = prefabCloneTransform.Find("in_junction");
            Vector3 offset = prefabCloneTransform.position - inJunction.position;
            foreach (Transform child in prefabCloneTransform)
                child.transform.position += offset;
            prefabCloneTransform.SetPositionAndRotation(swTransform.position, swTransform.rotation);
            GameObject throughTrack = sw.throughTrack.gameObject;
            GameObject divergingTrack = sw.divergingTrack.gameObject;
            throughTrack.transform.SetParent(prefabCloneTransform, false);
            divergingTrack.transform.SetParent(prefabCloneTransform, false);
            sw.tracksParent = prefabClone;
            Junction junction = inJunction.gameObject.AddComponent<Junction>();
            junction.selectedBranch = 1;
            prefabClone.GetComponentInChildren<VisualSwitch>().junction = junction;
            junction.inBranch = new Junction.Branch(sw.inTrack.GetComponent<RailTrack>(), sw.inTrackFirst);
            RailTrack throughRailTrack = throughTrack.GetComponent<RailTrack>();
            throughRailTrack.generateMeshes = false;
            RailTrack divergingRailTrack = divergingTrack.GetComponent<RailTrack>();
            divergingRailTrack.generateMeshes = false;
            junction.outBranches = new[] {
                new Junction.Branch(throughRailTrack, true),
                new Junction.Branch(divergingRailTrack, true)
            }.ToList();
            prefabClone.SetActive(true);
        }

        private static void ConnectRailTrack(Track track)
        {
            RailTrack railTrack = track.gameObject.GetComponent<RailTrack>();
            if (track.inTrack != null)
            {
                RailTrack inRailTrack = track.inTrack.GetComponent<RailTrack>();
                railTrack.inBranch = new Junction.Branch(inRailTrack, track.inTrackFirst);
            }

            if (track.outTrack != null)
            {
                RailTrack outRailTrack = track.outTrack.GetComponent<RailTrack>();
                railTrack.outBranch = new Junction.Branch(outRailTrack, track.outTrackFirst);
            }

            if (track.inSwitch)
                railTrack.inJunction = track.inSwitch.tracksParent.GetComponentInChildren<Junction>(true);

            if (track.outSwitch)
                railTrack.outJunction = track.outSwitch.tracksParent.GetComponentInChildren<Junction>(true);
        }

        private static void CreateSigns()
        {
            new GameObject("Signs").AddComponent<SignPlacer>();
        }

        private static void SetupGameScene()
        {
            Main.Logger.Log("Creating SaveLoadController");
            new GameObject("[LicensesAndGarages]").AddComponent<SaveLoadController>();
            Main.Logger.Log("Creating CarSpawner");
            new GameObject("[CarSpawner]").WithComponent<CarSpawner>().WithComponent<CarSpawnerOriginShiftHandler>();
            Main.Logger.Log("Creating ItemDisablerGrid");
            new GameObject("[JobLogicController]").AddComponent<LogicController>();
            Main.Logger.Log("Creating ItemDisablerGrid");
            new GameObject("[DerailAndDamageObserver]").AddComponent<DerailAndDamageObserver>();
            Main.Logger.Log("Creating StorageLogic");
            GameObject storageLogic = new GameObject("[StorageLogic]");
            storageLogic.NewChild("StorageWorld").WithComponentT<StorageBase>().storageType = StorageType.World;
            storageLogic.NewChild("StorageLostAndFound").WithComponentT<StorageBase>().storageType = StorageType.LostAndFound;
            storageLogic.NewChild("StorageInventory").WithComponentT<StorageBase>().storageType = StorageType.Inventory;
            storageLogic.NewChild("StorageBelt").WithComponentT<StorageBase>().storageType = StorageType.Belt;
            storageLogic.AddComponent<StorageController>(); // Must be added after all StorageBase's
            Main.Logger.Log("Creating ItemDisablerGrid");
            new GameObject("[ItemDisablerGrid]").AddComponent<ItemDisablerGrid>();
            Main.Logger.Log("Creating ShopLogic");
            GlobalShopController globalShopController = new GameObject("[ShopLogic]").AddComponent<GlobalShopController>();
            globalShopController.globalShopList = new List<Shop>();
            globalShopController.shopItemsData = new List<ShopItemData>();
            Main.Logger.Log("Creating RenderTextureSystem");
            new GameObject("[RenderTextureSystem]").AddComponent<RenderTextureSystem>();
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
