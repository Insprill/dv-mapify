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
        [MenuItem("Assets/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            if (!Directory.Exists(Application.streamingAssetsPath)) Directory.CreateDirectory(OUTPUT_DIR);
            BuildPipeline.BuildAssetBundles(OUTPUT_DIR, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
    }
}
