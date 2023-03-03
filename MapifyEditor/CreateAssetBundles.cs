using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public static class CreateAssetBundles
    {
        private const string OUTPUT_DIR = "Assets/Out";

        // todo: only build bundles for what we need, like what ccl does
        // todo: add buildtime check for exactly one MapInfo existing
        // todo: add buildtime check for scene names
        // todo: add buildtime check for required gameobjects
        // todo: add buildtime check for splines
        [MenuItem("Mapify/Export Map")]
        private static void BuildAllAssetBundles()
        {
            foreach (BezierCurve curve in Object.FindObjectsOfType<BezierCurve>())
            {
                curve.resolution = 0.5f;
                curve.close = false;
                if (curve.pointCount < 2)
                {
                    Debug.LogError("BezierCurve must have at least two points!", curve);
                    return;
                }

                for (int i = 0; i < curve.pointCount; i++)
                {
                    if (curve[i] != null) continue;
                    Debug.LogError("BezierCurve must have all points set!", curve);
                    return;
                }
            }

            TrackConnector.ConnectTracks();
            Debug.Log("Building AssetBundles");
            if (!Directory.Exists(OUTPUT_DIR)) Directory.CreateDirectory(OUTPUT_DIR);
            BuildPipeline.BuildAssetBundles(OUTPUT_DIR, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}
