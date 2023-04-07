using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public class GameContentSceneValidator : SceneValidator
    {
        protected override IEnumerator<Result> ValidateScene(Scene terrainScene, Scene railwayScene, Scene gameContentScene)
        {
            GameObject[] roots = gameContentScene.GetRootGameObjects();

            #region Lights

            Light[] lights = roots.SelectMany(go => go.GetComponentsInChildren<Light>()).ToArray();
            int directionalLightCount = lights.Count(light => light.type == LightType.Directional);
            if (directionalLightCount != 1)
                yield return Result.Error($"There must be exactly one directional light in the {GetPrettySceneName()} scene. Found {directionalLightCount}");

            #endregion

            #region Stations

            Dictionary<Station, List<WarehouseMachine>> warehouses = roots.SelectMany(go => go.GetComponentsInChildren<WarehouseMachine>()).MapToClosestStation();

            Track[] tracks = railwayScene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Track>()).ToArray();
            foreach (Station station in roots.SelectMany(go => go.GetComponentsInChildren<Station>()))
            {
                // Tracks
                station.storageTrackNames = new List<string>();
                station.transferInTrackNames = new List<string>();
                station.transferOutTrackNames = new List<string>();
                foreach (Track track in tracks)
                {
                    if (track.stationId != station.stationID)
                        continue;
                    switch (track.trackType)
                    {
                        case TrackType.Storage:
                            station.storageTrackNames.Add(track.name);
                            break;
                        case TrackType.In:
                            station.transferInTrackNames.Add(track.name);
                            break;
                        case TrackType.Out:
                            station.transferOutTrackNames.Add(track.name);
                            break;
                    }
                }

                // Teleport location
                if (station.teleportLocation == null)
                    yield return Result.Error($"You must set a teleport location for station {station.stationName}!", station);

                // Job booklet spawn area
                VanillaObject vanillaObject = station.GetComponentInSelfOrParent<VanillaObject>();
                if ((vanillaObject == null || !$"{vanillaObject.asset}".StartsWith("Station")) && station.bookletSpawnArea == null)
                    yield return Result.Error($"You must specify a job booklet spawn area for custom station {station.stationName}!", station);

                station.inputCargoGroupsCount = station.inputCargoGroups.Count;
                station.inputCargoGroups.ForEach(set => set.ToMonoBehaviour(station.gameObject));
                station.outputCargoGroups.ForEach(set => set.ToMonoBehaviour(station.gameObject));

                if (warehouses.TryGetValue(station, out List<WarehouseMachine> machines))
                {
                    foreach (WarehouseMachine machine in machines)
                        if (Track.Find(machine.loadingTrackStationId, machine.loadingTrackYardId, machine.loadingTrackId, TrackType.Loading) == null)
                            yield return Result.Error("Failed to find track loading track", machine);

                    station.warehouseMachines = machines;
                }
            }

            #endregion
        }

        public override void Cleanup()
        {
            foreach (CargoSetMonoBehaviour mb in Object.FindObjectsOfType<CargoSetMonoBehaviour>())
                Object.DestroyImmediate(mb);
        }

        public override string GetScenePath()
        {
            return "Assets/Scenes/GameContent.unity";
        }
    }
}
