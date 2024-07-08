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
            Station[] stations = scenes.gameContentScene.GetAllComponents<Station>();
            switch (stations.Length)
            {
                case 0:
                    yield return Result.Warning("No stations found! Things may not function as intended in-game without them. Unless the map is for testing purposes, you must fix this!");
                    break;
                case 1:
                    yield return Result.Warning("Only one station was found! Jobs will only generate between two stations. Unless the map is for testing purposes, you must fix this!");
                    break;
            }

            foreach (Station station in stations)
            {
                #region Station Info

                if (string.IsNullOrWhiteSpace(station.stationName))
                    yield return Result.Error($"Station '{station.name}' must have a name [{station.stationName}] ", station);

                if (string.IsNullOrWhiteSpace(station.stationID))
                    yield return Result.Error($"Station '{station.name}' must have an ID", station);

                var invalidCharacters = "";
                foreach (var character in station.stationID)
                {
                    if (!character.IsAsciiLetterOrDigit())
                    {
                        invalidCharacters += $"'{character}', ";
                    }
                }

                if (invalidCharacters != "")
                {
                    yield return Result.Error($"Station IDs can only contain letters and numbers. The ID '{station.stationID}' of station '{station.name}' is invalid. Invalid characters: {invalidCharacters}", station);
                }

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

                bool skipMachineChecks = false;
                for (var i = 0; i < station.warehouseMachines.Length; i++)
                {
                    var machine = station.warehouseMachines[i];
                    if (machine == null)
                    {
                        //machine set to null / None would cause a NullReferenceException below
                        skipMachineChecks = true;
                        yield return Result.Error($"Station has warehouse machine set to None", station);
                    }
                }

                if (!skipMachineChecks)
                {
                    Cargo[] warehouseCargoTypes = station.warehouseMachines.SelectMany(m => m.supportedCargoTypes).Distinct().ToArray();
                    Cargo[] stationCargoTypes = station.inputCargoGroups.Concat(station.outputCargoGroups).SelectMany(g => g.cargoTypes).Distinct().ToArray();

                    foreach (Cargo unusedCargo in warehouseCargoTypes.Except(stationCargoTypes))
                        yield return Result.Error($"Station has warehouse machine with {unusedCargo} but the station doesn't accept or output it!", station);

                    foreach (Cargo cargo in stationCargoTypes.Except(warehouseCargoTypes))
                        yield return Result.Error($"No WarehouseMachine found that accepts {cargo}!", station);
                }

                #endregion
            }
        }
    }
}

#endif
