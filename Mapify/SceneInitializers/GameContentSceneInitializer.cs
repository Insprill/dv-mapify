using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV;
using DV.RenderTextureSystem;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Station = Mapify.Editor.Station;

namespace Mapify.SceneInitializers
{
    public static class GameContentSceneInitializer
    {
        private static readonly FieldInfo StationController_Field_jobBookletSpawnSurface = AccessTools.DeclaredField(typeof(StationController), "jobBookletSpawnSurface");

        public static void SceneLoaded(Scene scene)
        {
            SetupGameScene();
            SetupVanillaAssets();
            SetupStations();
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(WorldMover.Instance.originShiftParent);
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
            // I'm not sure where vanilla creates this because it doesn't have auto create enabled.
            Main.Logger.Log("Creating YardTracksOrganizer");
            new GameObject("[YardTracksOrganizer]").AddComponent<YardTracksOrganizer>();
        }

        private static void SetupStations()
        {
            Station[] stations = Object.FindObjectsOfType<Station>();
            Dictionary<Station, List<LocomotiveSpawner>> locomotiveSpawners = Object.FindObjectsOfType<LocomotiveSpawner>().MapToClosestStation();

            foreach (Station station in stations)
            {
                GameObject stationObject = station.gameObject;
                stationObject.SetActive(false);
                stationObject.AddComponent<StationController>();
            }

            foreach (Station station in stations)
            {
                GameObject stationObject = station.gameObject;
                StationController stationController = stationObject.GetComponent<StationController>();

                // Station info
                stationController.stationInfo = new StationInfo(station.stationName, " ", station.stationID, station.color);

                // Station tracks
                stationController.storageRailtracksGONames = station.storageTrackNames;
                stationController.transferInRailtracksGONames = station.transferInTrackNames;
                stationController.transferOutRailtracksGONames = station.transferOutTrackNames;

                // Job booklet spawn surface
                PointOnPlane jobBookletSpawnSurface = stationObject.GetComponentInChildren<PointOnPlane>();
                if (jobBookletSpawnSurface == null)
                {
                    GameObject jobBookletSpawnSurfaceObject = stationObject.NewChildWithPosition("JobSpawnerAnchor", station.transform.TransformPoint(station.bookletSpawnArea.center));
                    jobBookletSpawnSurface = jobBookletSpawnSurfaceObject.AddComponent<PointOnPlane>();
                    Vector3 size = station.bookletSpawnArea.size;
                    jobBookletSpawnSurface.xSize = size.x;
                    jobBookletSpawnSurface.xSize = size.z;
                }

                StationController_Field_jobBookletSpawnSurface.SetValue(stationController, jobBookletSpawnSurface);

                // Job generation ranges
                // todo: should these be customizable?
                StationJobGenerationRange jobGenerationRange = stationObject.AddComponent<StationJobGenerationRange>();
                jobGenerationRange.stationCenterAnchor = station.yardCenter;

                // Job generation rules
                StationProceduralJobsRuleset proceduralJobsRuleset = stationObject.AddComponent<StationProceduralJobsRuleset>();
                proceduralJobsRuleset.inputCargoGroups = JsonUtility.FromJson<List<CargoSet>>(station.inputCargoGroupsSerialized).ToVanilla();
                proceduralJobsRuleset.outputCargoGroups = JsonUtility.FromJson<List<CargoSet>>(station.outputCargoGroupsSerialized).ToVanilla();
                proceduralJobsRuleset.jobsCapacity = station.jobsCapacity;
                proceduralJobsRuleset.minCarsPerJob = station.minCarsPerJob;
                proceduralJobsRuleset.maxCarsPerJob = station.maxCarsPerJob;
                proceduralJobsRuleset.maxShuntingStorageTracks = station.maxShuntingStorageTracks;
                proceduralJobsRuleset.haulStartingJobSupported = station.haulStartingJobSupported;
                proceduralJobsRuleset.loadStartingJobSupported = station.loadStartingJobSupported;
                proceduralJobsRuleset.unloadStartingJobSupported = station.unloadStartingJobSupported;
                proceduralJobsRuleset.emptyHaulStartingJobSupported = station.emptyHaulStartingJobSupported;

                // Warehouse machines
                //todo: this list gets broken when the warehouse machines get replaced with their vanilla counterparts
                stationController.warehouseMachineControllers = station.warehouseMachines.Select(machine =>
                {
                    WarehouseMachineController machineController = machine.gameObject.WithComponentT<WarehouseMachineController>();
                    machineController.warehouseTrackName = machine.warehouseTrackName;
                    machineController.supportedCargoTypes = machine.supportedCargoTypes.ToVanilla();
                    return machineController;
                }).ToList();

                // Teleport anchor
                Transform teleportAnchor = stationObject.NewChild("TeleportAnchor").transform;
                teleportAnchor.position = station.teleportLocation.position;
                teleportAnchor.rotation = station.teleportLocation.rotation;

                // Locomotive Spawners
                if (locomotiveSpawners.TryGetValue(station, out List<LocomotiveSpawner> spawners))
                    foreach (LocomotiveSpawner locomotiveSpawner in spawners)
                    {
                        GameObject gameObject = stationObject.NewChild("LocomotiveSpawner");
                        StationLocoSpawner locoSpawner = gameObject.AddComponent<StationLocoSpawner>();
                        locoSpawner.locoSpawnTrackName = locomotiveSpawner.Track.name;
                        locoSpawner.locoTypeGroupsToSpawn = locomotiveSpawner.condensedLocomotiveTypes
                            .Select(rollingStockTypes =>
                                new ListTrainCarTypeWrapper(rollingStockTypes.Split(',').Select(rollingStockType =>
                                        (TrainCarType)Enum.Parse(typeof(TrainCarType), rollingStockType)
                                    ).ToList()
                                )
                            ).ToList();
                    }

                stationObject.SetActive(true);
            }
        }

        private static void SetupVanillaAssets()
        {
            foreach (VanillaObject vanillaObject in Object.FindObjectsOfType<VanillaObject>())
                switch (vanillaObject.asset)
                {
                    case VanillaAsset.CareerManager:
                    case VanillaAsset.JobValidator:
                    case VanillaAsset.TrashCan:
                    case VanillaAsset.Dumpster:
                    case VanillaAsset.LostAndFoundShed:
                    case VanillaAsset.StationOffice1:
                    case VanillaAsset.StationOffice2:
                    case VanillaAsset.StationOffice3:
                    case VanillaAsset.StationOffice4:
                    case VanillaAsset.StationOffice5:
                    case VanillaAsset.StationOffice6:
                    case VanillaAsset.StationOffice7:
                        vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset), new[] { typeof(Station) });
                        break;
                    case VanillaAsset.WarehouseMachine:
                        WarehouseMachine machine = vanillaObject.GetComponent<WarehouseMachine>();
                        Station[] stations = Object.FindObjectsOfType<Station>().Where(s => s.warehouseMachines.Contains(machine)).ToArray();
                        machine = vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset), new[] { typeof(WarehouseMachine) }).GetComponent<WarehouseMachine>();
                        foreach (Station station in stations)
                            for (int i = 0; i < station.warehouseMachines.Count; i++)
                                if (station.warehouseMachines[i] == null)
                                    station.warehouseMachines[i] = machine;
                        break;
                }
        }
    }
}
