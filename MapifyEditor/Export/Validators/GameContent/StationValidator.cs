#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;

namespace Mapify.Editor.Validators
{
    public class StationValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            Dictionary<Station, List<WarehouseMachine>> warehouses = scenes.gameContentScene.GetAllComponents<WarehouseMachine>().MapToClosestStation();

            Station[] stations = scenes.gameContentScene.GetAllComponents<Station>();
            if (stations.Length == 0)
                yield return Result.Warning("No stations found! Things may not function as intended in-game without them. Unless the map is for testing purposes, you must fix this!");
            else if (stations.Length == 1)
                yield return Result.Warning("Only one station was found! Jobs will only generate between two stations. Unless the map is for testing purposes, you must fix this!");

            foreach (Station station in stations)
            {
                #region Station Info

                if (string.IsNullOrWhiteSpace(station.stationName))
                    yield return Result.Error($"Station '{station.name}' must have a name", station);
                if (string.IsNullOrWhiteSpace(station.stationID))
                    yield return Result.Error($"Station '{station.name}' must have an ID", station);
                if (station.color.a < 0.001)
                    yield return Result.Error($"Station '{station.name}' must have a color with an alpha value greater than 0", station);

                #endregion

                #region Teleporters

                if (station.teleportLocation == null)
                    yield return Result.Error($"You must set a teleport location for station {station.stationName}!", station);

                #endregion

                #region Job Booklets

                VanillaObject vanillaObject = station.GetComponentInSelfOrParent<VanillaObject>();
                if ((vanillaObject == null || !$"{vanillaObject.asset}".StartsWith("Station")) && station.bookletSpawnArea == null)
                    yield return Result.Error($"You must specify a job booklet spawn area for custom station {station.stationName}!", station);

                #endregion

                #region Warehouse Machines

                if (warehouses.TryGetValue(station, out List<WarehouseMachine> machines))
                    foreach (Cargo cargo in station.inputCargoGroups.Concat(station.outputCargoGroups).SelectMany(g => g.cargoTypes).Except(machines.SelectMany(m => m.supportedCargoTypes)))
                        yield return Result.Error($"No WarehouseMachine found that accepts {cargo}!", station);

                #endregion
            }
        }
    }
}
#endif
