#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;

namespace MapifyEditor.Export.Validators
{
    public class LocomotiveSpawnerValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            LocomotiveSpawner[] spawners = new[] {
                scenes.railwayScene,
                scenes.gameContentScene
            }.SelectMany(s => s.GetAllComponents<LocomotiveSpawner>()).ToArray();

            // this is kinda gross

            #region All Spawners

            foreach (LocomotiveSpawner spawner in spawners)
            {
                Track selfTrack = spawner.GetComponent<Track>();
                if (!selfTrack && !spawner.GetComponentInParent<Station>())
                    yield return Result.Error($"Locomotive Spawner's must be the child of a {nameof(Station)} or have a {nameof(Track)} assigned!", spawner);
                else if (!selfTrack && spawner.Track == null)
                    yield return Result.Error($"Failed to find parking track with Station ID '{spawner.loadingTrackStationId}', Yard ID '{spawner.loadingTrackYardId}', Track ID '{spawner.loadingTrackId}'", spawner);
            }

            #endregion

            #region Vanilla Spawners

            foreach (VanillaLocomotiveSpawner spawner in spawners.OfType<VanillaLocomotiveSpawner>())
                if (spawner.locomotiveGroups.Count == 0)
                    yield return Result.Error("Locomotive spawners must have at least one group to spawn!", spawner);
                else if (spawner.locomotiveGroups.Exists(group => group.rollingStock.Count == 0))
                    yield return Result.Error("Locomotive spawner groups must have at least one type to spawn!", spawner);

            #endregion

            #region Custom Spawners

            foreach (CustomLocomotiveSpawner spawner in spawners.OfType<CustomLocomotiveSpawner>())
                if (spawner.locomotiveGroups.Count == 0)
                    yield return Result.Error("Locomotive spawners must have at least one group to spawn!", spawner);
                else if (spawner.locomotiveGroups.Exists(group => group.rollingStock.Count == 0))
                    yield return Result.Error("Locomotive spawner groups must have at least one type to spawn!", spawner);

            #endregion
        }
    }
}
#endif
