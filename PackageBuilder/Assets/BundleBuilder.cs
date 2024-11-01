using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class BundleBuilder : MonoBehaviour
{
    public string bundleName;
    public string exportPath;

    public void BuildIt()
    {
        var builds = new List<AssetBundleBuild>();
        var asssetBundleFiles = new List<string>();

        if (bundleName.Any(Char.IsUpper))
        {
            Debug.LogWarning("Upper case letters are automatically changed to lower case letters by Unity!");
        }

        Debug.Log("Building asset bundle. Files:");

        foreach (var assPath in AssetDatabase.GetAllAssetPaths())
        {
            //the / at the end is crucial
            if (!assPath.StartsWith("Assets/BuildThis/"))
            {
                continue;
            }

            asssetBundleFiles.Add(assPath);
            Debug.Log(assPath);
        }

        if (!asssetBundleFiles.Any())
        {
            Debug.LogError("Empty asset bundle!");
            return;
        }

        builds.Add(new AssetBundleBuild {
            assetBundleName = bundleName,
            assetNames = asssetBundleFiles.ToArray()
        });

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
