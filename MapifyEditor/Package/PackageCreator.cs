#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public static class PackageCreator
    {
        private const string UNITY_PACKAGE_EXT_NAME = "unitypackage";
        private const string EXPORT_ASSET_PATH = "Assets/Mapify";
        private const string LAST_EXPORTED_KEY = "Mapify.Package.LastExportPath";
        private const string LAST_IMPORTED_KEY = "Mapify.Package.LastImportPath";

        [MenuItem("Mapify/Package/Export", priority = 4)]
        public static void Export()
        {
            string path = EditorPrefs.GetString(LAST_EXPORTED_KEY);
            if (string.IsNullOrWhiteSpace(path) || Directory.GetParent(path)?.Exists == false)
            {
                path = EditorUtility.SaveFilePanel("Package", "../", $"mapify.{UNITY_PACKAGE_EXT_NAME}", UNITY_PACKAGE_EXT_NAME);
                if (string.IsNullOrWhiteSpace(path)) return;
                EditorPrefs.SetString(LAST_EXPORTED_KEY, path);
            }

            if (File.Exists(EXPORT_ASSET_PATH))
                File.Delete(EXPORT_ASSET_PATH);

            AssetDatabase.ExportPackage(EXPORT_ASSET_PATH, path, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log($"Package exported to '{path}'!");
        }

        [MenuItem("Mapify/Package/Import", priority = 3)]
        public static void Import()
        {
            string path = EditorPrefs.GetString(LAST_IMPORTED_KEY);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                path = EditorUtility.OpenFilePanel("Package", "../", $"{UNITY_PACKAGE_EXT_NAME}");
                if (string.IsNullOrWhiteSpace(path)) return;
                EditorPrefs.SetString(LAST_IMPORTED_KEY, path);
            }

            AssetDatabase.ImportPackage(path, false);
            Debug.Log($"Package imported from '{path}'!");
        }

        [MenuItem("Mapify/Package/Remove Saved Paths", priority = 5)]
        public static void RemoveSavedPath()
        {
            EditorPrefs.DeleteKey(LAST_EXPORTED_KEY);
            EditorPrefs.DeleteKey(LAST_IMPORTED_KEY);
            Debug.Log("Removed saved paths.");
        }
    }
}
#endif
