#if UNITY_EDITOR
using System.Collections.Generic;
using Mapify.Editor.Utils;

namespace Mapify.Editor.Validators
{
    public class WarehouseMachineValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            WarehouseMachine[] warehouseMachines = scenes.railwayScene.GetAllComponents<WarehouseMachine>();
            foreach (WarehouseMachine warehouseMachine in warehouseMachines)
                if (warehouseMachine.LoadingTrack == null)
                    yield return Result.Error(
                        $"Failed to find loading track with Station ID '{warehouseMachine.loadingTrackStationId}', Yard ID '{warehouseMachine.loadingTrackYardId}', Track ID '{warehouseMachine.loadingTrackId}'",
                        warehouseMachine
                    );
        }
    }
}
#endif
