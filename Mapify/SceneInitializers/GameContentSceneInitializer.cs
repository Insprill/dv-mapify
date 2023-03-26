using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV;
using DV.RenderTextureSystem;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers
{
    public static class GameContentSceneInitializer
    {
        private static readonly FieldInfo StationController_Field_jobBookletSpawnSurface = AccessTools.DeclaredField(typeof(StationController), "jobBookletSpawnSurface");

        public static void SceneLoaded()
        {
            SetupGameScene();
            SetupStations();
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

        private static void SetupStations()
        {
            Station[] stations = Object.FindObjectsOfType<Station>();
            Dictionary<Station, List<LocomotiveSpawner>> locomotiveSpawners = Object
                .FindObjectsOfType<LocomotiveSpawner>()
                .GroupBy(spawner => spawner.closestStation)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (Station station in stations)
            {
                GameObject stationObject = station.gameObject;
                stationObject.SetActive(false);
                StationController stationController = stationObject.AddComponent<StationController>();

                // Station info
                stationController.stationInfo = new StationInfo(station.name, station.type, station.yardID, station.color);

                // Station tracks
                stationController.storageRailtracksGONames = station.storageTracks.ToNames().ToList();
                stationController.transferInRailtracksGONames = station.transferInTracks.ToNames().ToList();
                stationController.transferOutRailtracksGONames = station.transferOutTracks.ToNames().ToList();

                // Job booklet spawn surface
                GameObject jobBookletSpawnSurfaceObject = stationObject.NewChildWithPosition("JobSpawnerAnchor", station.transform.TransformPoint(station.bookletSpawnArea.center));
                PointOnPlane jobBookletSpawnSurface = jobBookletSpawnSurfaceObject.AddComponent<PointOnPlane>();
                Vector3 size = station.bookletSpawnArea.size;
                jobBookletSpawnSurface.xSize = size.x;
                jobBookletSpawnSurface.xSize = size.z;
                StationController_Field_jobBookletSpawnSurface.SetValue(stationController, jobBookletSpawnSurface);

                // Job generation ranges.
                // todo: should these be customizable?
                stationObject.AddComponent<StationJobGenerationRange>();

                // todo: this
                stationObject.AddComponent<StationProceduralJobsRuleset>();

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
                        locoSpawner.locoTypeGroupsToSpawn = locomotiveSpawner.locomotiveTypesToSpawn.Select(rollingStockTypes =>
                            new ListTrainCarTypeWrapper(rollingStockTypes.Select(rollingStockType =>
                                    (TrainCarType)Enum.Parse(typeof(TrainCarType), $"{rollingStockTypes}")
                                ).ToList()
                            )
                        ).ToList();
                    }

                stationObject.SetActive(true);
            }
        }
    }
}
