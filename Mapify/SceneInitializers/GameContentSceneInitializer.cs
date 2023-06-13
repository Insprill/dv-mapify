using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.CashRegister;
using DV.Printers;
using DV.Shops;
using DV.Teleporters;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mapify.SceneInitializers
{
    public static class GameContentSceneInitializer
    {
        #region Store Values

        private const float STORE_Y_ROT_OFFSET = 90;

        #endregion

        private static readonly FieldInfo StationController_Field_jobBookletSpawnSurface = AccessTools.DeclaredField(typeof(StationController), "jobBookletSpawnSurface");
        private static readonly MethodInfo LogicController_Method_GetCarTypesThatStationUses = AccessTools.DeclaredMethod(
            typeof(LogicController),
            "GetCarTypesThatStationUses",
            new[] { typeof(StationController) }
        );

        public static void SceneLoaded(Scene scene)
        {
            foreach (Transform transform in scene.GetRootGameObjects().Select(go => go.transform))
                transform.SetParent(WorldMover.Instance.originShiftParent);
            SetupGameScene();
            SetupZoneBlockers();
            SetupVanillaAssets();
            SetupStations();
            SetupPitstops();
            SetupStores();
            CreateWater();
            SetupPostProcessing();
        }

        private static void SetupGameScene()
        {
            AssetCopier.Instantiate(VanillaAsset.ItemBuoyancyEnabler, false);
            AssetCopier.Instantiate(VanillaAsset.ItemDisablerGrid, false);
            AssetCopier.Instantiate(VanillaAsset.JobLogicController, false);
            AssetCopier.Instantiate(VanillaAsset.LicensesAndGarages, false);
            AssetCopier.Instantiate(VanillaAsset.ShopLogic, false, false);
            AssetCopier.Instantiate(VanillaAsset.SingletonInstanceFinder, false);
            AssetCopier.Instantiate(VanillaAsset.StorageLogic, false);
            AssetCopier.Instantiate(VanillaAsset.UnderwaterPostProcessing, false);
            AssetCopier.Instantiate(VanillaAsset.WeatherDV, false);
            AssetCopier.Instantiate(VanillaAsset.WeatherGUITogglerScript, false);
            // I'm not sure where vanilla creates this because it doesn't have auto create enabled.
            Mapify.Log("Creating YardTracksOrganizer");
            new GameObject("[YardTracksOrganizer]").AddComponent<YardTracksOrganizer>();
        }

        private static void SetupZoneBlockers()
        {
            foreach (AreaBlocker areaBlocker in Object.FindObjectsOfType<AreaBlocker>())
            {
                GameObject go = areaBlocker.gameObject;
                StationLicenseZoneBlocker zoneBlocker = go.AddComponent<StationLicenseZoneBlocker>();
                zoneBlocker.requiredJobLicense = areaBlocker.requiredLicense.ConvertByName<JobLicense, JobLicenses>().ToV2();
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
                StationJobGenerationRange jobGenerationRange = stationObject.AddComponent<StationJobGenerationRange>();
                jobGenerationRange.stationCenterAnchor = station.YardCenter;
                jobGenerationRange.generateJobsSqrDistance = station.jobGenerationDistance * station.jobGenerationDistance;
                jobGenerationRange.destroyGeneratedJobsSqrDistanceRegular = station.jobDestroyDistance * station.jobDestroyDistance;
                jobGenerationRange.jobOverviewBookletGenerationSqrDistance = station.bookletGenerationDistance * station.bookletGenerationDistance;

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
                proceduralJobsRuleset.haulStartingJobSupported = station.generateFreightHaul;
                proceduralJobsRuleset.loadStartingJobSupported = station.generateShuntingLoad;
                proceduralJobsRuleset.unloadStartingJobSupported = station.generateShuntingUnload;
                proceduralJobsRuleset.emptyHaulStartingJobSupported = station.generateLogisticalHaul;

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
                                        ((TrainCarType)Enum.Parse(typeof(TrainCarType), rollingStockType)).ToV2()
                                    ).ToList()
                                )
                            ).ToList();
                    }

                // Todo: If this doesn't pick up our stuff, can we just re-call the Awake and Start functions?
                // logicController.YardIdToStationController.Add(stationController.stationInfo.YardID, stationController);
                // logicController.stationToSupportedCarTypes.Add(stationController,
                //     (HashSet<TrainCarType_v2>)LogicController_Method_GetCarTypesThatStationUses.Invoke(logicController, new object[] { stationController }));

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
                    case VanillaAsset.PlayerHouse:
                        vanillaObject.Replace();
                        break;
                    case VanillaAsset.StationOffice1:
                    case VanillaAsset.StationOffice2:
                    case VanillaAsset.StationOffice3:
                    case VanillaAsset.StationOffice4:
                    case VanillaAsset.StationOffice5:
                    case VanillaAsset.StationOffice6:
                    case VanillaAsset.StationOffice7:
                        GameObject go = vanillaObject.Replace();
                        Transform youAreHereFlag = go.transform.FindChildByName("youarehere_flag");
                        if (youAreHereFlag != null)
                            Object.Destroy(youAreHereFlag.gameObject);
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
                    serviceStation.PositionThing(manualServiceIndicatorTransform, moduleObj.transform, i);
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
                        vanillaObject.Replace();
                    }
                    else if (asset == VanillaAsset.CashRegister)
                    {
                        GameObject cashRegisterObj = vanillaObject.Replace(keepChildren: false);
                        // CashRegisterWithModules cashRegister = cashRegisterObj.GetComponentInChildren<CashRegisterWithModules>();
                        // cashRegister.resourceMachines = resourceModules.Cast<ResourceModule>().ToArray();
                    }
                }

                serviceStation.gameObject.Replace(pitStopStationObject).SetActive(true);
            }
        }

        private static void SetupStores()
        {
            GlobalShopController gsc = SingletonBehaviour<GlobalShopController>.Instance;
            gsc.globalShopList = new List<Shop>();

            foreach (Store store in Object.FindObjectsOfType<Store>())
            {
                Transform shopTransform = AssetCopier.Instantiate(VanillaAsset.Store).transform;
                PlayerDistanceMultipleGameObjectsOptimizer optimizer = shopTransform.GetComponent<PlayerDistanceMultipleGameObjectsOptimizer>();
                optimizer.gameObjectsToDisable = new List<GameObject>();

                foreach (Transform child in store.cashRegister.GetChildren())
                {
                    Renderer r = child.GetComponent<Renderer>();
                    if (r != null) Object.Destroy(r);
                    child.SetParent(shopTransform, false);
                }

                store.itemSpawnReference.localPosition = store.itemSpawnReference.localPosition.SwapAndInvertXZ();

                Transform meshTransform = AssetCopier.Instantiate(VanillaAsset.StoreMesh, active: false).transform;
                shopTransform.SetParent(meshTransform, false);
                shopTransform.localPosition += store.cashRegister.localPosition;
                shopTransform.eulerAngles = shopTransform.eulerAngles.AddY(STORE_Y_ROT_OFFSET);

                Shop shop = shopTransform.GetComponent<Shop>();
                shop.itemSpawnTransform = store.itemSpawnReference;
                gsc.globalShopList.Add(shop);


                shop.scanItemResourceModules = new ScanItemCashRegisterModule[store.itemTypes.Length];
                for (int i = 0; i < store.itemTypes.Length; i++)
                {
                    Transform t = AssetCopier.Instantiate(store.itemTypes[i].ToVanillaAsset()).transform;
                    t.SetParent(store.moduleReference, false);
                    store.PositionThing(store.moduleReference, t, i);
                    shop.scanItemResourceModules[i] = t.GetComponent<ScanItemCashRegisterModule>();
                    optimizer.gameObjectsToDisable.Add(t.gameObject);
                }

                CashRegisterWithModules cashRegister = shopTransform.GetComponentInChildren<CashRegisterWithModules>();
                cashRegister.registerModules = shop.scanItemResourceModules.Cast<CashRegisterModule>().ToArray();
                optimizer.gameObjectsToDisable.Add(cashRegister.gameObject);
                cashRegister.GetComponent<PrinterController>().spawnAnchor = store.itemSpawnReference;

                optimizer.gameObjectsToDisable.Add(shopTransform.FindChildByName("ScannerAnchor").gameObject);

                store.gameObject.Replace(meshTransform.gameObject).SetActive(true);
            }

            gsc.gameObject.SetActive(true);
        }

        private static void SetupPostProcessing()
        {
            GameObject obj = GameObject.Find("[GlobalPostProcessing]") ?? new GameObject("[GlobalPostProcessing]");
            Layer.PostProcessing.Apply(obj);
            PostProcessVolume volume = obj.WithComponent<PostProcessVolume>();
            volume.isGlobal = true;
            PostProcessProfile profile = volume.profile;
            if (!profile.HasSettings<AmbientOcclusion>()) profile.AddSettings<AmbientOcclusion>();
            if (!profile.HasSettings<AutoExposure>()) profile.AddSettings<AutoExposure>();
            if (!profile.HasSettings<Bloom>()) profile.AddSettings<Bloom>();
        }
    }
}
