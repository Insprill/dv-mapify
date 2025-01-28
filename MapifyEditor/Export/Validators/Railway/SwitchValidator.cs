#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEngine;

namespace MapifyEditor.Export.Validators
{
    public class SwitchValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            foreach (var switch_ in scenes.railwayScene.GetAllComponents<SwitchBase>())
            {
                var switchTracks = switch_.GetTracks();
                if (switchTracks.Length < 2)
                {
                    yield return Result.Error($"Switches must have at least 2 branches but it has {switchTracks.Length}", switch_);
                    continue;
                }

                if (switchTracks.Any(switchTrack => switchTrack == null))
                {
                    yield return Result.Error($"Switch track is null", switch_);
                    continue;
                }

                var jointPointPos = switch_.GetJointPoint().position;
                for (int i = 1; i < switchTracks.Length; i++)
                {
                    if (Vector3.Distance(jointPointPos, switchTracks[i].Curve[0].position) <= Track.SNAP_RANGE) continue;

                    yield return Result.Error("All tracks in switches must connect to each other at point 0", switch_);
                    break;
                }

                foreach (var track in switchTracks)
                {
                    track.Snap();
                    if (track.isInSnapped && track.isOutSnapped) continue;

                    yield return Result.Error("Switches must have a track attached to all points", track);
                }
            }
        }
    }
}
#endif
