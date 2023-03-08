using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    // Me: Can we have CCLs TrainCarValidator?
    // Mom: We have CCLs TrainCarValidator at home
    // TrainCarValidator at home:
    public static class MapValidator
    {
        // todo: add check for scene names
        // todo: add check for required gameobjects
        public static IEnumerator<Result> Validate()
        {
            // MapInfo
            MapInfo[] mapInfos = AssetDatabase.FindAssets($"t:{nameof(MapInfo)}").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<MapInfo>).ToArray();
            if (mapInfos.Length > 1) yield return Result.Error($"There should only be one MapInfo! Found {mapInfos.Length}");
            if (mapInfos.Length == 0) yield return Result.Error("Missing MapInfo");

            // BezierCurves
            foreach (BezierCurve curve in Object.FindObjectsOfType<BezierCurve>())
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
        }

        public class Result
        {
            public Object context;
            public string message;

            private Result(string message, Object context)
            {
                this.message = message;
                this.context = context;
            }

            public static Result Error(string message, Object context = null)
            {
                return new Result(message, context);
            }
        }
    }
}
