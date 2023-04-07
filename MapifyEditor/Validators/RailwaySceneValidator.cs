using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public class RailwaySceneValidator : SceneValidator
    {
        protected override IEnumerator<Result> ValidateScene(Scene terrainScene, Scene railwayScene, Scene gameContentScene)
        {
            GameObject[] roots = railwayScene.GetRootGameObjects();

            if (roots.Length == 0)
            {
                yield return Result.Error($"The {GetPrettySceneName()} scene must contain a [railway] object");
                yield break;
            }

            if (roots.Length > 1)
            {
                yield return Result.Error($"The {GetPrettySceneName()} scene's only root object should be [railway]");
                yield break;
            }

            # region Track

            List<Track> tracks = roots.SelectMany(go => go.GetComponentsInChildren<Track>()).ToList();
            if (tracks.Count == 0)
            {
                yield return Result.Error("Failed to find any track!");
                yield break;
            }

            if (roots[0].name != "[railway]")
                yield return Result.Error($"Unknown object {roots[0].name} in {GetPrettySceneName()} scene. The only object should be [railway]", roots[0]);

            Station[] stations = gameContentScene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Station>()).ToArray();

            int roadId = 1;
            bool anyFailed = false;
            foreach (Track track in tracks)
            {
                track.Snap();
                if (track.IsSwitch)
                    continue;
                switch (track.trackType)
                {
                    case TrackType.Road:
                        // Tracks starting with [Y] or [#] don't get signs generated (SignPlacer#ShouldIncludeTrack)
                        track.name = $"{(track.generateSigns ? "" : "[#] ")}Road {roadId++}";
                        continue; // Road safety laws prepare to be ignored!
                    //todo: give each of these a colour
                    case TrackType.Storage:
                        break;
                    case TrackType.Loading:
                        break;
                    case TrackType.In:
                        break;
                    case TrackType.Out:
                        break;
                    case TrackType.Parking:
                        break;
                    case TrackType.PassengerStorage:
                        break;
                    case TrackType.PassengerLoading:
                        break;
                }

                bool fail = false;
                if (string.IsNullOrWhiteSpace(track.stationId))
                {
                    yield return Result.Error("Station ID not specified", track);
                    fail = true;
                }
                else if (stations.All(station => station.stationID != track.stationId))
                {
                    yield return Result.Error($"Failed to find station with ID {track.yardId}", track);
                    fail = true;
                }

                if (track.trackId < 1 || track.trackId > 99)
                {
                    yield return Result.Error("Track ID must be between 1 and 99 (inclusive)", track);
                    fail = true;
                }

                if (!fail)
                    track.name = $"[Y]_[{track.stationId}]_[{track.yardId}-{track.trackId:D2}-{track.trackType.LetterId()}]";
                else
                    anyFailed = true;
            }

            if (!anyFailed)
            {
                List<Track> duplicateTracks = tracks.Where(x => x.trackType != TrackType.Road)
                    .GroupBy(x => $"{x.stationId}-{x.yardId}-{x.trackId}-{x.trackType}")
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g)
                    .ToList();
                foreach (Track duplicate in duplicateTracks)
                    yield return Result.Error($"Duplicate track {duplicate.name}", duplicate);
            }

            #endregion

            #region Bezier Curves

            foreach (BezierCurve curve in roots.SelectMany(go => go.GetComponentsInChildren<BezierCurve>()))
            {
                curve.resolution = 0.5f;
                curve.close = false;
                if (curve.pointCount < 2)
                    yield return Result.Error("BezierCurve must have at least two points!", curve);

                for (int i = 0; i < curve.pointCount; i++)
                {
                    if (curve[i] != null) continue;
                    yield return Result.Error("BezierCurve must have all points set!", curve);
                }
            }

            foreach (BezierPoint point in roots.SelectMany(go => go.GetComponentsInChildren<BezierPoint>()))
                if (point.transform.localEulerAngles != Vector3.zero)
                    yield return Result.Error("BezierPoint must not be rotated!", point);

            #endregion

            # region Switches

            foreach (Switch sw in Object.FindObjectsOfType<Switch>())
            {
                VanillaObject vanillaObject = sw.GetComponent<VanillaObject>();
                vanillaObject.asset = sw.DivergingTrack.GetComponent<BezierCurve>().Last().localPosition.x < 0
                    ? sw.standSide == Switch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchLeftOuterSign
                        : VanillaAsset.SwitchLeft
                    : sw.standSide == Switch.StandSide.DIVERGING
                        ? VanillaAsset.SwitchRightOuterSign
                        : VanillaAsset.SwitchRight;
                Track divergingTrack = sw.DivergingTrack.GetComponent<Track>();
                Track throughTrack = sw.ThroughTrack.GetComponent<Track>();
                if (!divergingTrack.isInSnapped || !divergingTrack.isOutSnapped || !throughTrack.isInSnapped || !throughTrack.isOutSnapped)
                    yield return Result.Error("Switches must have a track attached to all points", sw);
            }

            #endregion

            #region Locomotive Spawners

            foreach (LocomotiveSpawner spawner in roots.SelectMany(go => go.GetComponentsInChildren<LocomotiveSpawner>()))
                if (spawner.locomotiveTypesToSpawn.Count == 0)
                    yield return Result.Error("Locomotive spawners must have at least one group to spawn!", spawner);
                else if (spawner.locomotiveTypesToSpawn.Any(group => group.rollingStockTypes.Count == 0))
                    yield return Result.Error("Locomotive spawner groups must have at least one type to spawn!", spawner);
                else
                    spawner.condensedLocomotiveTypes = spawner.locomotiveTypesToSpawn.Select(types => string.Join(",", types.rollingStockTypes.Select(type => type.ToString()))).ToList();

            #endregion
        }

        public override string GetScenePath()
        {
            return "Assets/Scenes/Railway.unity";
        }
    }
}
