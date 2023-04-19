using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;

namespace Mapify.Editor.Validators
{
    public class WarehouseMachineValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            WarehouseMachine[] warehouseMachines = scenes.railwayScene.GetAllComponents<WarehouseMachine>();
            Dictionary<Station, List<WarehouseMachine>> stationMap = warehouseMachines.MapToClosestStation();
            foreach (WarehouseMachine warehouseMachine in warehouseMachines)
            {
                if (warehouseMachine.LoadingTrack == null)
                    yield return Result.Error(
                        $"Failed to find loading track with Station ID '{warehouseMachine.loadingTrackStationId}', Yard ID '{warehouseMachine.loadingTrackYardId}', Track ID '{warehouseMachine.loadingTrackId}'",
                        warehouseMachine
                    );
                Station station = stationMap.FirstOrDefault(kvp => kvp.Value.Contains(warehouseMachine)).Key;
                if (station == null)
                    yield return Result.Error("Failed to find Station associated with warehouse machine! Are you sure one exists?", warehouseMachine);
                else
                    foreach (Cargo cargo in warehouseMachine.supportedCargoTypes.Except(station.inputCargoGroups.Concat(station.outputCargoGroups).SelectMany(g => g.cargoTypes)))
                        yield return Result.Error($"Station doesn't accept or output {cargo}!", warehouseMachine);
            }
        }
    }
}
