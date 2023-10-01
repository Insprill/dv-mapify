#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Mapify.Editor.StateUpdaters;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Object = UnityEngine.Object;

namespace Mapify.Editor
{
    public static class MapExporter
    {
        private const string MODS_PATH = "steamapps/common/Derail Valley/Mods";

        private static string LastReleaseExportPath {
            get => EditorPrefs.GetString("Mapify.Export.Release.LastExportPath");
            set => EditorPrefs.SetString("Mapify.Export.Release.LastExportPath", value);
        }

        private static string LastDebugExportPath {
            get => EditorPrefs.GetString("Mapify.Export.Debug.LastExportPath");
            set => EditorPrefs.SetString("Mapify.Export.Debug.LastExportPath", value);
        }

        public static void OpenExportPrompt(bool releaseMode)
        {
            string mapName = EditorAssets.FindAsset<MapInfo>()?.name;
            if (releaseMode)
                ExportRelease(mapName);
            else
                ExportDebug(mapName);
        }

        private static void ExportRelease(string mapName)
        {
            string startingPath;
            string name;
            if (!string.IsNullOrEmpty(LastReleaseExportPath) && Directory.GetParent(LastReleaseExportPath)?.Exists == true)
            {
                startingPath = Path.GetDirectoryName(LastReleaseExportPath);
                name = Path.GetFileName(LastReleaseExportPath);
            }
            else
            {
                startingPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                name = $"{mapName}.zip";
            }

            string exportFilePath = EditorUtility.SaveFilePanel("Export Map", startingPath, name, "zip");
            if (string.IsNullOrWhiteSpace(exportFilePath))
                return;
            LastReleaseExportPath = exportFilePath;

            string tmpFolder = Path.Combine(Path.GetTempPath(), $"dv-mapify-{Path.GetRandomFileName()}");
            if (Directory.Exists(tmpFolder))
                Directory.Delete(tmpFolder, true);
            Directory.CreateDirectory(tmpFolder);

            bool success = Export(tmpFolder, false);

            if (success)
            {
                EditorUtility.DisplayProgressBar("Mapify", "Creating zip file", 0);
                ZipFile.CreateFromDirectory(tmpFolder, exportFilePath, CompressionLevel.Fastest, true);
                EditorUtility.ClearProgressBar();
                if (EditorUtility.DisplayDialog("Mapify", "Export complete!", "Open Folder", "Ok"))
                    EditorUtility.RevealInFinder(exportFilePath);
            }

            if (Directory.Exists(tmpFolder))
                Directory.Delete(tmpFolder, true);
        }

        private static void ExportDebug(string mapName)
        {
            string startingPath;
            string name;
            if (!string.IsNullOrEmpty(LastDebugExportPath) && Directory.Exists(LastDebugExportPath))
            {
                startingPath = Path.GetDirectoryName(LastDebugExportPath);
                name = Path.GetFileName(LastDebugExportPath);
            }
            else
            {
                startingPath = GetDefaultMapsFolder();
                name = mapName;
            }

            string exportFolderPath = EditorUtility.SaveFolderPanel("Export Map", startingPath, name);
            if (string.IsNullOrWhiteSpace(exportFolderPath))
                return;
            LastDebugExportPath = exportFolderPath;

            bool success = Export(exportFolderPath, true);

            if (success && EditorUtility.DisplayDialog("Mapify", "Export complete!", "Open Folder", "Ok"))
                EditorUtility.RevealInFinder(exportFolderPath);
        }

        private static bool Export(string rootExportDir, bool uncompressed)
        {
            MapInfo mapInfo = EditorAssets.FindAsset<MapInfo>();
            mapInfo.mapifyVersion = File.ReadLines("Assets/Mapify/version.txt").First().Trim();

            string mapExportDir = Path.Combine(rootExportDir, mapInfo.name);

            DirectoryInfo mapExportDirInfo = new DirectoryInfo(mapExportDir);

            if (mapExportDirInfo.Exists && (mapExportDirInfo.GetFiles().Length > 0 || mapExportDirInfo.GetDirectories().Length > 0))
            {
                int result = EditorUtility.DisplayDialogComplex("Clear Folder",
                    "The map's export directory isn't empty. " +
                    "If you've exported this map before, you may skip this to improve export speed. " +
                    "If you've made significant changes to your map, or the files are from something else, " +
                    "you should either move the files to trash or cancel and see what they are.",
                    "Move to Trash",
                    "Cancel",
                    "Skip");
                switch (result)
                {
                    case 0:
                        EditorFileUtil.MoveToTrash(mapExportDir);
                        break;
                    case 1:
                        return false;
                }
            }

            mapExportDirInfo.Create();

            BuildUpdater.Update();

            AssetBundleBuild[] builds = CreateBuilds(EditorSceneManager.GetSceneByPath(Scenes.TERRAIN));

            bool success = BuildPipeline.BuildAssetBundles(
                mapExportDir,
                builds,
                uncompressed ? BuildAssetBundleOptions.UncompressedAssetBundle : BuildAssetBundleOptions.None,
                BuildTarget.StandaloneWindows64
            );

            BuildUpdater.Cleanup();

            string mapInfoPath = Path.Combine(mapExportDir, Names.MAP_INFO_FILE);
            string modInfoPath = Path.Combine(rootExportDir, Names.MOD_INFO_FILE);
            if (!success)
            {
                Debug.LogError("Build was canceled or failed!");
                File.Delete(mapInfoPath); // Prevents the mod from loading an incomplete asset bundle
            }
            else
            {
                CreateMapInfo(mapInfoPath, mapInfo);
                CreateModInfo(modInfoPath, mapInfo);
            }

            return success;
        }

        private static AssetBundleBuild[] CreateBuilds(Scene terrainScene)
        {
            Terrain[] sortedTerrain = terrainScene.GetAllComponents<Terrain>()
                .Where(terrain => terrain.gameObject.activeInHierarchy)
                .ToArray()
                .Sort();

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>(sortedTerrain.Length + 2);
            for (int i = 0; i < sortedTerrain.Length; i++)
                builds.Add(new AssetBundleBuild {
                    assetBundleName = $"terraindata_{i}",
                    assetNames = new[] { AssetDatabase.GetAssetPath(sortedTerrain[i].terrainData) }
                });

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            List<string> assetPaths = new List<string>(allAssetPaths.Length - sortedTerrain.Length);
            List<string> scenePaths = new List<string>();
            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string assetPath = allAssetPaths[i];
                if (!assetPath.StartsWith("Assets/")) continue;
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null || importer is MonoImporter || importer is PluginImporter) continue;
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (obj is TerrainData) continue;
                (obj is SceneAsset ? scenePaths : assetPaths).Add(assetPath);

                EditorUtility.DisplayProgressBar("Gathering assets", assetPath, i / (float)allAssetPaths.Length);
            }

            EditorUtility.ClearProgressBar();

            //put big assets in their own assetbundle to avoid the combined assetbundle getting too big.
            //Unity cannot load assetbundles larger then 4GB.
            for (var i = 0; i < assetPaths.Count; i++)
            {
                string absolutePath = Path.GetFullPath(assetPaths[i]);

                //skip directories
                if ((File.GetAttributes(absolutePath) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    continue;
                }

                long fileSize = new FileInfo(absolutePath).Length;

                if (fileSize > 500*1000000) //500MB
                {
                    Debug.Log("Putting asset "+assetPaths[i]+" in it's own bundle because it's large: "+fileSize/1000000+" MB");

                    string[] pathArray = { assetPaths[i] };
                    builds.Add(new AssetBundleBuild {
                        assetBundleName = Names.ASSETS_ASSET_BUNDLES_PREFIX+'_'+Path.GetFileName(absolutePath),
                        assetNames = pathArray
                    });

                    //make sure it doesn't get built twice
                    assetPaths[i] = null;
                }
            }

            //remove null
            assetPaths.RemoveAll(item => item == null);

            //build the other assets
            builds.Add(new AssetBundleBuild {
                assetBundleName = Names.ASSETS_ASSET_BUNDLES_PREFIX,
                assetNames = assetPaths.ToArray()
            });
            builds.Add(new AssetBundleBuild {
                assetBundleName = Names.SCENES_ASSET_BUNDLE,
                assetNames = scenePaths.ToArray()
            });

            return builds.ToArray();
        }

        private static void CreateMapInfo(string filePath, MapInfo mapInfo)
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(BasicMapInfo.FromMapInfo(mapInfo)));
        }

        private static void CreateModInfo(string filePath, MapInfo mapInfo)
        {
            UnityModManagerInfo modInfo = new UnityModManagerInfo {
                Id = mapInfo.name,
                Version = mapInfo.version,
                DisplayName = mapInfo.name,
                ManagerVersion = "0.27.3",
                Requirements = new[] { "Mapify" },
                HomePage = mapInfo.homePage
            };
            File.WriteAllText(filePath, JsonUtility.ToJson(modInfo));
        }

        private static string GetDefaultMapsFolder()
        {
            // search for the user's DV install
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return GetWindowsMapsFolder();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return GetLinuxMapsFolder();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to find default save path");
                Debug.LogException(e);
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private static string GetWindowsMapsFolder()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                string driveRoot = drive.RootDirectory.FullName;
                foreach (string s in new[] { "Program Files", "Program Files (x86)" })
                {
                    string potentialPath = Path.Combine(driveRoot, s, "Steam", MODS_PATH);
                    if (Directory.Exists(potentialPath))
                        return potentialPath;
                }
            }

            return null;
        }

        private static string GetLinuxMapsFolder()
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string potentialPath = Path.Combine(homePath, ".steam", "steam", MODS_PATH);
            return Directory.Exists(potentialPath) ? potentialPath : null;
        }
    }
}
#endif
