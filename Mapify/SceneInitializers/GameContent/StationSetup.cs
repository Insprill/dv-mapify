using System.Linq;
using System.Reflection;
using DV.Teleporters;
using DV.ThingTypes;
using DV.Utils;
using HarmonyLib;
using Mapify.Editor;
using Mapify.SceneInitializers.Railway;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.GameContent
{
    public class StationSetup : SceneSetup
    {
        private static readonly FieldInfo StationController_Field_jobBookletSpawnSurface = AccessTools.DeclaredField(typeof(StationController), "jobBookletSpawnSurface");

        public override void Run()
        {
            Station[] stations = Object.FindObjectsOfType<Station>();

            stations.SetActive(false);

            foreach (Station station in stations)
                station.gameObject.AddComponent<StationController>();

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

                SetupJobBookletSpawnSurface(station, stationController);
                SetupJobGenerationRange(station);
                SetupJobRules(station, stationController);
                SetupWarehouseMachines(station, stationController);
                SetupTeleportAnchor(station);
                SetupLocomotiveSpawners(station);
            }

            stations.SetActive(true);
            SingletonBehaviour<LogicController>.Instance.gameObject.SetActive(true);
        }

        private static void SetupJobBookletSpawnSurface(Station station, StationController stationController)
        {
            PointOnPlane jobBookletSpawnSurface = station.transform.parent.GetComponentInChildren<PointOnPlane>();

            if (jobBookletSpawnSurface == null)
            {
                GameObject jobBookletSpawnSurfaceObject = station.gameObject.NewChildWithPosition("JobSpawnerAnchor", station.transform.TransformPoint(station.bookletSpawnArea.center));
                jobBookletSpawnSurface = jobBookletSpawnSurfaceObject.AddComponent<PointOnPlane>();
                Vector3 size = station.bookletSpawnArea.size;
                jobBookletSpawnSurface.xSize = size.x;
                jobBookletSpawnSurface.zSize = size.z;
            }

            StationController_Field_jobBookletSpawnSurface.SetValue(stationController, jobBookletSpawnSurface);
        }

        private static void SetupJobGenerationRange(Station station)
        {
            StationJobGenerationRange jobGenerationRange = station.gameObject.AddComponent<StationJobGenerationRange>();
            jobGenerationRange.stationCenterAnchor = station.YardCenter;
            jobGenerationRange.generateJobsSqrDistance = station.jobGenerationDistance * station.jobGenerationDistance;
            jobGenerationRange.destroyGeneratedJobsSqrDistanceRegular = station.jobDestroyDistance * station.jobDestroyDistance;
            jobGenerationRange.jobOverviewBookletGenerationSqrDistance = station.bookletGenerationDistance * station.bookletGenerationDistance;
        }

        private static void SetupJobRules(Station station, StationController stationController)
        {
            StationProceduralJobsRuleset proceduralJobsRuleset = station.gameObject.AddComponent<StationProceduralJobsRuleset>();
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
        }

        private static void SetupWarehouseMachines(Station station, StationController stationController)
        {
            stationController.warehouseMachineControllers = station.warehouseMachines.Select(machine =>
            {
                WarehouseMachineController machineController = machine.GetComponentInParent<WarehouseMachineController>();
                machineController.warehouseTrackName = machine.LoadingTrack.name;
                machineController.supportedCargoTypes = machine.supportedCargoTypes.ConvertByName<Cargo, CargoType>();
                return machineController;
            }).ToList();
        }

        private static void SetupTeleportAnchor(Station station)
        {
            Transform teleportAnchor = station.gameObject.NewChild("TeleportAnchor").transform;
            teleportAnchor.position = station.teleportLocation.position;
            teleportAnchor.rotation = station.teleportLocation.rotation;
            StationTeleporter teleporter = teleportAnchor.gameObject.AddComponent<StationTeleporter>();
            teleporter.playerTeleportAnchor = teleportAnchor;
            teleporter.playerTeleportMapMarkerAnchor = teleportAnchor;
        }

        private static void SetupLocomotiveSpawners(Station station)
        {
            foreach (LocomotiveSpawner spawner in station.GetComponentsInChildren<LocomotiveSpawner>())
                LocomotiveSpawnerSetup.SetupLocomotiveSpawner(spawner);
        }
    }
}
