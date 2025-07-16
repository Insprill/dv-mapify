#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;
using UnityEditor;
using UnityEngine;

namespace MapifyEditor.Export.Validators
{
    public class TrackValidator : Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            Track[] tracks = scenes.railwayScene.GetAllComponents<Track>();

            if (tracks.Length == 0)
            {
                yield return Result.Error($"Failed to find any tracks in the {scenes.railwayScene.name} scene!");
                yield break;
            }

            Station[] stations = scenes.gameContentScene.GetAllComponents<Station>();
            foreach (Track track in tracks)
            {
                track.Snap();
                if (track.IsSwitch || track.IsTurntable)
                    continue;

                if (PrefabUtility.IsPartOfPrefabInstance(track))
                    yield return Result.Warning("Track prefabs should be unpacked completely before being used", track);

                if (track.trackType == TrackType.Road)
                {
                    if (!string.IsNullOrWhiteSpace(track.stationId))
                    {
                        yield return Result.Warning($"Track {track.name} will not be assigned to specified station {track.stationId} because {nameof(track.trackType)} is set to {track.trackType}", track);
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(track.stationId))
                        yield return Result.Error("Station ID not specified", track);
                    else if (stations.All(station => station.stationID != track.stationId))
                        yield return Result.Error($"Failed to find station with ID {track.stationId}", track);
                    if (track.trackId < 1 || track.trackId > 99)
                        yield return Result.Error("Track ID must be between 1 and 99 (inclusive)", track);
                }

                if (track.Curve.pointCount < 2)
                    yield return Result.Error("BezierCurve must have at least two points!", track.Curve);

                for (int i = 0; i < track.Curve.pointCount; i++)
                {
                    BezierPoint point = track.Curve[i];
                    if (point == null)
                    {
                        yield return Result.Error("BezierCurve must have all points set!", track.Curve);
                    }
                    else
                    {
                        if (point.transform.localEulerAngles != Vector3.zero)
                            yield return Result.Error("BezierPoint must not be rotated!", point);
                    }
                }
            }

            List<Track> duplicateTracks = tracks.Where(x => x.trackType != TrackType.Road)
                .GroupBy(x => $"{x.stationId}-{x.yardId}-{x.trackId}-{x.trackType}")
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToList();
            foreach (Track duplicate in duplicateTracks)
                yield return Result.Error($"Duplicate track {duplicate.name}", duplicate);
        }
    }
}
#endif
