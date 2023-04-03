using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public class RailwaySceneValidator : SceneValidator
    {
        protected override IEnumerator<Result> ValidateScene(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            # region Track

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

            if (roots[0].name != "[railway]") yield return Result.Error($"Unknown object {roots[0].name} in {GetPrettySceneName()} scene. The only object should be [railway]", roots[0]);
            if (roots[0] != null && roots[0].GetComponentsInChildren<Track>().Length == 0) yield return Result.Error("Failed to find any track!");

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

            #region Locomotive Spawners

            foreach (LocomotiveSpawner spawner in roots.SelectMany(go => go.GetComponentsInChildren<LocomotiveSpawner>()))
                if (spawner.locomotiveTypesToSpawn.Count == 0)
                    yield return Result.Error("Locomotive spawners must have at least one group to spawn!", spawner);
                else if (spawner.locomotiveTypesToSpawn.Count(group => group.rollingStockTypes.Count == 0) != 0)
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
