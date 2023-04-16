using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.Logic.Job;
using HarmonyLib;
using PassengerJobsMod;
using UnityEngine;
using Track = Mapify.Editor.Track;

namespace Mapify.Patches.Mods
{
    public static class PassengerJobsPatch
    {
        public static void Patch(Harmony harmony)
        {
            MethodInfo PassengerJobGenerator_Initialize_Patch_Prefix = AccessTools.DeclaredMethod(typeof(PassengerJobGenerator_Initialize_Patch), nameof(PassengerJobGenerator_Initialize_Patch.Prefix));
            MethodInfo PassengerJobGenerator_Initialize = AccessTools.DeclaredMethod(typeof(PassengerJobGenerator), nameof(PassengerJobGenerator.Initialize));
            harmony.Patch(PassengerJobGenerator_Initialize, new HarmonyMethod(PassengerJobGenerator_Initialize_Patch_Prefix));

            MethodInfo StationProceduralJobsController_Awake_Patch_Prefix =
                AccessTools.DeclaredMethod(typeof(StationProceduralJobsController_Awake_Patch), nameof(StationProceduralJobsController_Awake_Patch.Prefix));
            MethodInfo StationProceduralJobsController_Awake = AccessTools.DeclaredMethod(typeof(StationProceduralJobsController), "Awake");
            harmony.Patch(StationProceduralJobsController_Awake, new HarmonyMethod(StationProceduralJobsController_Awake_Patch_Prefix));
        }

        private static class PassengerJobGenerator_Initialize_Patch
        {
            private static bool initialized;

            internal static bool Prefix()
            {
                if (initialized) return true;
                initialized = true;

                Dictionary<string, HashSet<string>> storageTrackNames = PassengerJobGenerator.StorageTrackNames;
                storageTrackNames.Clear();
                Dictionary<string, HashSet<string>> platformTrackNames = PassengerJobGenerator.PlatformTrackNames;
                platformTrackNames.Clear();
                Dictionary<string, string[]> commuterDestinations = PassengerJobGenerator.CommuterDestinations;
                commuterDestinations.Clear();

                RailTrack[] tracks = RailTrackRegistry.AllTracks;
                foreach (StationController station in StationController.allStations)
                {
                    string stationId = station.stationInfo.YardID;
                    storageTrackNames.Add(stationId, tracks
                        .Where(t => t.GetComponent<Track>()?.trackType == Editor.TrackType.PassengerStorage)
                        .Select(t => t.logicTrack.ID)
                        .Where(id => id.yardId == stationId)
                        .Select(id => id.ToString())
                        .ToHashSet());
                    platformTrackNames.Add(stationId, tracks
                        .Where(t => t.GetComponent<Track>()?.trackType == Editor.TrackType.PassengerLoading)
                        .Select(t => t.logicTrack.ID)
                        .Where(id => id.yardId == stationId)
                        .Select(id => id.ToString())
                        .ToHashSet());
                    foreach (CargoGroup cargoGroup in station.proceduralJobsRuleset.outputCargoGroups.Where(group => group.cargoTypes.Contains(CargoType.Passengers)))
                        commuterDestinations.Add(stationId, cargoGroup.stations.Select(s => s.stationInfo.YardID).ToArray());
                }

                return true;
            }
        }

        private static class StationProceduralJobsController_Awake_Patch
        {
            internal static bool Prefix(StationProceduralJobsController __instance)
            {
                if (__instance.GetComponent<PassengerJobGenerator>() != null)
                    return true;

                bool isPassengerStation = Object.FindObjectsOfType<Track>()
                    .GroupBy(t => t.stationId)
                    .Any(g => g.Any(t => t.trackType == Editor.TrackType.PassengerLoading) && g.Any(t => t.trackType == Editor.TrackType.PassengerStorage));
                if (!isPassengerStation)
                    return true;

                __instance.gameObject.AddComponent<PassengerJobGenerator>().Initialize();
                return true;
            }
        }
    }
}
