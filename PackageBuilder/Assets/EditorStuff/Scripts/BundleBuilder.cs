using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

#if UNITY_EDITOR

public class BundleBuilder : MonoBehaviour
{
    public void BuildIt()
    {
        var builds = new List<AssetBundleBuild>();
        var assetBundleFiles = new List<string>();

        Debug.Log("Building asset bundle. Files:");

        foreach (var assPath in AssetDatabase.GetAllAssetPaths())
        {
            if (!assPath.StartsWith("Assets/AssetBundleContent/"))
            {
                continue;
            }

            assetBundleFiles.Add(assPath);
            Debug.Log(assPath);
        }

        if (!assetBundleFiles.Any())
        {
            Debug.LogError("Empty asset bundle!");
            return;
        }

        builds.Add(new AssetBundleBuild {
            assetBundleName = Mapify.Editor.Names.ASSET_BUNDLE_NAME,
            assetNames = assetBundleFiles.ToArray()
        });

        // Application.dataPath points to the Assets folder. We export the bundle to the build folder.
        var exportPath = $"{Application.dataPath}/../../build/assetbundles/";
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        Debug.Log($"Exporting to {exportPath}");

        var success = BuildPipeline.BuildAssetBundles(
            exportPath,
            builds.ToArray(),
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );

        Debug.Log($"success: {success}");
    }
}

#endif
