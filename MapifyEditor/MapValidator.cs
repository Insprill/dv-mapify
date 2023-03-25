using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mapify.Editor.Utils;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.Editor
{
    // Me: Can we have CCLs TrainCarValidator?
    // Mom: We have CCLs TrainCarValidator at home
    // TrainCarValidator at home:
    public static class MapValidator
    {
        private const string MAP_NAME_REGEX = "[a-zA-Z0-9-_& ]";

        // todo: add check for scene names
        // todo: add check for required gameobjects
        public static IEnumerator<Result> Validate()
        {
            // MapInfo
            MapInfo[] mapInfos = EditorAssets.FindAllAssets<MapInfo>();
            if (mapInfos.Length > 1) yield return Result.Error($"There should only be one MapInfo! Found {mapInfos.Length}");
            if (mapInfos.Length == 0) yield return Result.Error("Missing MapInfo");
            if (mapInfos.Length == 1 && !Regex.IsMatch(mapInfos[0].mapName, MAP_NAME_REGEX)) yield return Result.Error($"Your map name must match the following pattern: {MAP_NAME_REGEX}");

            // Railway scene
            IEnumerator<Result> validateRailwayScene = ValidateRailwayScene();
            while (validateRailwayScene.MoveNext()) yield return validateRailwayScene.Current;
        }

        private static IEnumerator<Result> ValidateRailwayScene()
        {
            const string scenePath = "Assets/Scenes/Railway.unity";
            Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
            if (scene.IsValid()) yield return Result.Error($"Failed to find Railway scene! It should be located at \"{scenePath}\"");
            bool isTerrainSceneLoaded = !scene.IsValid() || scene.isLoaded;
            if (!isTerrainSceneLoaded)
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            GameObject railway = GameObject.Find("[railway]");
            if (railway == null) yield return Result.Error("Failed to find [railway] object in the Railway scene!");
            if (railway != null && railway.GetComponentsInChildren<Track>().Length == 0) yield return Result.Error("Failed to find any track!");

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

            if (!isTerrainSceneLoaded)
                EditorSceneManager.UnloadSceneAsync(scenePath);
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
