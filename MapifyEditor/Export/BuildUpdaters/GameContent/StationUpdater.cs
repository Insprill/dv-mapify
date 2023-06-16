#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.StateUpdaters
{
    public class StationUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            Dictionary<Station, List<WarehouseMachine>> warehouses = scenes.gameContentScene.GetAllComponents<WarehouseMachine>().MapToClosestStation();
            Track[] nonRoadTracks = scenes.railwayScene.GetAllComponents<Track>().Where(t => t.trackType != TrackType.Road).ToArray();

            foreach (Station station in scenes.gameContentScene.GetAllComponents<Station>())
            {
                #region Trackage

                station.storageTrackNames = new List<string>();
                station.transferInTrackNames = new List<string>();
                station.transferOutTrackNames = new List<string>();
                foreach (Track track in nonRoadTracks)
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

                #endregion

                #region Jobs

                station.inputCargoGroupsCount = station.inputCargoGroups.Count;
                station.inputCargoGroups.ForEach(set => set.ToMonoBehaviour(station.gameObject));
                station.outputCargoGroups.ForEach(set => set.ToMonoBehaviour(station.gameObject));

                #endregion

                #region Warehouse Machines

                if (warehouses.TryGetValue(station, out List<WarehouseMachine> machines))
                    station.warehouseMachines = machines;

                #endregion
            }
        }

        protected override void Cleanup(Scenes scenes)
        {
            foreach (CargoSetMonoBehaviour mb in scenes.gameContentScene.GetAllComponents<CargoSetMonoBehaviour>())
                Object.DestroyImmediate(mb);
        }
    }
}
#endif
