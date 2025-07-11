#if UNITY_EDITOR

using System;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor.Tools
{
    public class VisualSettings: ScriptableObject
    {
        private const string TRACK_FOLDER = PackageCreator.EXPORT_ASSET_PATH + "/Meshes/Trackage";

        public bool EnableTrackVisuals = true;
        public GameObject TrackPreviewPrefab;

        public bool DrawGizmosLine = true;

        private static VisualSettings _instance;
        public static VisualSettings Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = EditorAssets.FindAsset<VisualSettings>();
                    if (!_instance)
                    {
                        _instance = Create();
                    }
                }

                return _instance;
            }
        }

        private static VisualSettings Create()
        {
            var newInstance = CreateInstance<VisualSettings>();
            AssetDatabase.CreateAsset(newInstance, PackageCreator.EXPORT_ASSET_PATH+"/VisualSettings.asset");

            var candidates = AssetDatabase.FindAssets("visualtrack t:Model", new []{TRACK_FOLDER});
            if (candidates.Length == 0)
            {
                Debug.LogWarning(nameof(VisualSettings)+": can't find default track prefab");
                return newInstance;
            }

            try
            {
                newInstance.TrackPreviewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(candidates[0]));
            }
            catch (Exception e)
            {
                Debug.LogWarning(nameof(VisualSettings)+": can't load track prefab: "+e.Message);
            }

            return newInstance;
        }
    }
}

#endif
