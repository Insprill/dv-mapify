using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.CashRegister;
using DV.Logic.Job;
using DV.Teleporters;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Station = Mapify.Editor.Station;

namespace Mapify.SceneInitializers
{
    public static class GameContentSceneInitializer
    {
        private static readonly FieldInfo StationController_Field_jobBookletSpawnSurface = AccessTools.DeclaredField(typeof(StationController), "jobBookletSpawnSurface");
        private static readonly MethodInfo LogicController_Method_GetContainerTypesThatStationUses = AccessTools.DeclaredMethod(
            typeof(LogicController),
            "GetContainerTypesThatStationUses",
            new[] { typeof(StationController) }
        );

        public static void SceneLoaded(Scene scene)
        {
            SetupGameScene();
            SetupZoneBlockers();
            SetupVanillaAssets();
            SetupStations();
            SetupPitstops();
            CreateWater();
            SetupPostProcessing();
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(WorldMover.Instance.originShiftParent);
        }

        private static void SetupGameScene()
        {
            AssetCopier.Instantiate(VanillaAsset.RenderTextureSystem, false);
            AssetCopier.Instantiate(VanillaAsset.CarSpawner, false);
            AssetCopier.Instantiate(VanillaAsset.LicensesAndGarages, false);
            AssetCopier.Instantiate(VanillaAsset.ItemDisablerGrid, false);
            AssetCopier.Instantiate(VanillaAsset.JobLogicController, false);
            AssetCopier.Instantiate(VanillaAsset.StorageLogic, false);
            GlobalShopController shopController = AssetCopier.Instantiate(VanillaAsset.ShopLogic, false).GetComponent<GlobalShopController>();
            shopController.globalShopList = new List<Shop>();
            shopController.shopItemsData = new List<ShopItemData>();
            AssetCopier.Instantiate(VanillaAsset.DerailAndDamageObserver, false);
            // I'm not sure where vanilla creates this because it doesn't have auto create enabled.
            Main.Logger.Log("Creating YardTracksOrganizer");
            new GameObject("[YardTracksOrganizer]").AddComponent<YardTracksOrganizer>();
        }

        private static void SetupZoneBlockers()
        {
            foreach (AreaBlocker areaBlocker in Object.FindObjectsOfType<AreaBlocker>())
            {
                GameObject go = areaBlocker.gameObject;
                StationLicenseZoneBlocker zoneBlocker = go.AddComponent<StationLicenseZoneBlocker>();
                zoneBlocker.requiredJobLicense = areaBlocker.requiredLicense.ConvertByName<JobLicense, JobLicenses>();
                zoneBlocker.blockerObjectsParent = go;
                InvalidTeleportLocationReaction reaction = go.AddComponent<InvalidTeleportLocationReaction>();
                reaction.blocker = zoneBlocker;
                zoneBlocker.tag = "NO_TELEPORT";
            }
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

            LogicController logicController = SingletonBehaviour<LogicController>.Instance;
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
                PointOnPlane jobBookletSpawnSurface = stationObject.transform.parent.GetComponentInChildren<PointOnPlane>();
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
                stationController.proceduralJobsRuleset = proceduralJobsRuleset;
                CargoSetMonoBehaviour[] mbs = station.GetComponents<CargoSetMonoBehaviour>();
                proceduralJobsRuleset.inputCargoGroups = mbs.Take(station.inputCargoGroupsCount).Select(mb => mb.ToOriginal()).ToVanilla();
                proceduralJobsRuleset.outputCargoGroups = mbs.Skip(station.inputCargoGroupsCount).Select(mb => mb.ToOriginal()).ToVanilla();
                proceduralJobsRuleset.jobsCapacity = station.jobsCapacity;
                proceduralJobsRuleset.minCarsPerJob = station.minCarsPerJob;
                proceduralJobsRuleset.maxCarsPerJob = station.maxCarsPerJob;
                proceduralJobsRuleset.maxShuntingStorageTracks = station.maxShuntingStorageTracks;
                proceduralJobsRuleset.haulStartingJobSupported = station.haulStartingJobSupported;
                proceduralJobsRuleset.loadStartingJobSupported = station.loadStartingJobSupported;
                proceduralJobsRuleset.unloadStartingJobSupported = station.unloadStartingJobSupported;
                proceduralJobsRuleset.emptyHaulStartingJobSupported = station.emptyHaulStartingJobSupported;

                // Warehouse machines
                stationController.warehouseMachineControllers = station.warehouseMachines.Select(machine =>
                {
                    WarehouseMachineController machineController = machine.GetComponentInParent<WarehouseMachineController>();
                    machineController.warehouseTrackName = machine.LoadingTrack.name;
                    machineController.supportedCargoTypes = machine.supportedCargoTypes.ConvertByName<Cargo, CargoType>();
                    return machineController;
                }).ToList();

                // Teleport anchor
                Transform teleportAnchor = stationObject.NewChild("TeleportAnchor").transform;
                teleportAnchor.position = station.teleportLocation.position;
                teleportAnchor.rotation = station.teleportLocation.rotation;
                StationTeleporter teleporter = teleportAnchor.gameObject.AddComponent<StationTeleporter>();
                teleporter.playerTeleportAnchor = teleportAnchor;
                teleporter.playerTeleportMapMarkerAnchor = teleportAnchor;

                // Locomotive Spawners
                if (locomotiveSpawners.TryGetValue(station, out List<LocomotiveSpawner> spawners))
                    foreach (LocomotiveSpawner locomotiveSpawner in spawners)
                    {
                        GameObject gameObject = stationObject.NewChild("LocomotiveSpawner");
                        StationLocoSpawner locoSpawner = gameObject.AddComponent<StationLocoSpawner>();
                        locoSpawner.spawnRotationFlipped = locomotiveSpawner.flipOrientation;
                        locoSpawner.locoSpawnTrackName = locomotiveSpawner.Track.name;
                        locoSpawner.locoTypeGroupsToSpawn = locomotiveSpawner.condensedLocomotiveTypes
                            .Select(rollingStockTypes =>
                                new ListTrainCarTypeWrapper(rollingStockTypes.Split(',').Select(rollingStockType =>
                                        (TrainCarType)Enum.Parse(typeof(TrainCarType), rollingStockType)
                                    ).ToList()
                                )
                            ).ToList();
                    }

                logicController.YardIdToStationController.Add(stationController.stationInfo.YardID, stationController);
                logicController.stationToSupportedContainerTypes.Add(stationController,
                    (HashSet<CargoContainerType>)LogicController_Method_GetContainerTypesThatStationUses.Invoke(logicController, new object[] { stationController }));

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
                    case VanillaAsset.WarehouseMachine:
                    case VanillaAsset.StationOffice1:
                    case VanillaAsset.StationOffice2:
                    case VanillaAsset.StationOffice3:
                    case VanillaAsset.StationOffice4:
                    case VanillaAsset.StationOffice5:
                    case VanillaAsset.StationOffice6:
                    case VanillaAsset.StationOffice7:
                    case VanillaAsset.PlayerHouse:
                        vanillaObject.gameObject.Replace(AssetCopier.Instantiate(vanillaObject.asset));
                        break;
                }
        }

        private static void CreateWater()
        {
            GameObject water = AssetCopier.Instantiate(VanillaAsset.Water);
            water.transform.position = new Vector3(0, SingletonBehaviour<LevelInfo>.Instance.waterLevel, 0);
        }

        private static void SetupPitstops()
        {
            ServiceStation[] serviceStations = Object.FindObjectsOfType<ServiceStation>();
            foreach (ServiceStation serviceStation in serviceStations)
            {
                serviceStation.transform.SetParent(WorldMover.Instance.originShiftParent);

                GameObject pitStopStationObject = AssetCopier.Instantiate(VanillaAsset.PitStopStation, active: false);
                Transform serviceStationTransform = serviceStation.transform;
                pitStopStationObject.transform.SetPositionAndRotation(serviceStationTransform.position, serviceStationTransform.rotation);

                GameObject manualServiceIndicator = pitStopStationObject.FindChildByName("ManualServiceIndicator");
                Transform manualServiceIndicatorTransform = manualServiceIndicator.transform;

                Transform msi = serviceStation.ManualServiceIndicator;
                manualServiceIndicatorTransform.SetPositionAndRotation(msi.position, msi.rotation);
                Object.Destroy(msi.gameObject);

                //todo: customizable price-per-unit
                List<LocoResourceModule> resourceModules = new List<LocoResourceModule>(serviceStation.resources.Length);
                for (int i = 0; i < serviceStation.resources.Length; i++)
                {
                    ServiceResource resource = serviceStation.resources[i];
                    GameObject moduleObj = AssetCopier.Instantiate(resource.ToVanillaAsset());
                    serviceStation.PositionRefillMachine(manualServiceIndicatorTransform, moduleObj.transform, i);
                    LocoResourceModule resourceModule = moduleObj.GetComponentInChildren<LocoResourceModule>();
                    resourceModules.Add(resourceModule);
                }

                PitStopIndicators pitStopIndicators = pitStopStationObject.GetComponentInChildren<PitStopIndicators>();
                pitStopIndicators.resourceModules = resourceModules.ToArray();

                PitStop pitStop = pitStopStationObject.GetComponentInChildren<PitStop>();

                VanillaObject[] vanillaObjects = serviceStation.GetComponentsInChildren<VanillaObject>();
                foreach (VanillaObject vanillaObject in vanillaObjects)
                {
                    VanillaAsset asset = vanillaObject.asset;
                    if (asset == serviceStation.markerType.ToVanillaAsset())
                    {
                        BoxCollider vCollider = vanillaObject.GetComponent<BoxCollider>();
                        BoxCollider collider = pitStop.gameObject.AddComponent<BoxCollider>();
                        collider.center = vCollider.center;
                        collider.size = vCollider.size;
                        collider.isTrigger = true;
                        vanillaObject.gameObject.Replace(AssetCopier.Instantiate(asset));
                    }
                    else if (asset == VanillaAsset.CashRegister)
                    {
                        GameObject cashRegisterObj = vanillaObject.gameObject.Replace(AssetCopier.Instantiate(asset), keepChildren: false);
                        CashRegisterResourceModules cashRegister = cashRegisterObj.GetComponentInChildren<CashRegisterResourceModules>();
                        cashRegister.resourceMachines = resourceModules.Cast<ResourceModule>().ToArray();
                    }
                }

                serviceStation.gameObject.Replace(pitStopStationObject).SetActive(true);
            }
        }

        private static void SetupPostProcessing()
        {
            GameObject obj = GameObject.Find("[GlobalPostProcessing]") ?? new GameObject("[GlobalPostProcessing]");
            obj.layer = LayerMask.NameToLayer("PostProcessing");
            PostProcessVolume volume = obj.WithComponentT<PostProcessVolume>();
            volume.isGlobal = true;
            PostProcessProfile profile = volume.profile;
            if (!profile.HasSettings<AmbientOcclusion>()) profile.AddSettings<AmbientOcclusion>();
            if (!profile.HasSettings<AutoExposure>()) profile.AddSettings<AutoExposure>();
            if (!profile.HasSettings<Bloom>()) profile.AddSettings<Bloom>();
        }
    }
}
