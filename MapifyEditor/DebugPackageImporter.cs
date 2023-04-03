using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public static class DebugPackageImporter
    {
        private const string UNITY_PACKAGE_EXT_NAME = "unitypackage";
        private const string EXPORT_ASSET_PATH = "Assets/Mapify";
        private const string LAST_IMPORTED_KEY = "Mapify.Package.LastImportPath";

        [MenuItem("Mapify/Debug/Package/Export")]
        public static void Export()
        {
            AssetDatabase.ExportPackage(EXPORT_ASSET_PATH, $"mapify.{UNITY_PACKAGE_EXT_NAME}", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log("Package exported!");
        }

        [MenuItem("Mapify/Debug/Package/Import")]
        public static void Import()
        {
            string dir = EditorPrefs.GetString(LAST_IMPORTED_KEY);
            if (string.IsNullOrWhiteSpace(dir))
            {
                dir = EditorUtility.OpenFilePanel("Package", "../", $"{UNITY_PACKAGE_EXT_NAME}");
                if (string.IsNullOrWhiteSpace(dir)) return;
                EditorPrefs.SetString(LAST_IMPORTED_KEY, dir);
            }

            AssetDatabase.ImportPackage(dir, false);
            Debug.Log("Package imported!");
        }

        [MenuItem("Mapify/Debug/Package/Remove Saved Path")]
        public static void RemoveSavedPath()
        {
            EditorPrefs.DeleteKey(LAST_IMPORTED_KEY);
            Debug.Log("Removed last imported path. You'll be prompted to pick it again next time you import the package.");
        }
    }
}
