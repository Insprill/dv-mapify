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
            if (Directory.Exists(tmpFolder)) {
                Directory.Delete(tmpFolder, true);
            }
            Directory.CreateDirectory(tmpFolder);

            string mapExportFolder = Path.Combine(tmpFolder, mapName);
            Directory.CreateDirectory(mapExportFolder);

            bool success = Export(mapExportFolder, false);

            if (success)
            {
                EditorUtility.DisplayProgressBar("Mapify", "Creating zip file", 0);

                if (File.Exists(exportFilePath))
                {
                    EditorFileUtil.MoveToTrash(exportFilePath);
                }

                ZipFile.CreateFromDirectory(mapExportFolder, exportFilePath, CompressionLevel.Fastest, true);
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
            mapInfo.mapifyVersion = File.ReadLines(Names.MAPIFY_VERSION_FILE).First().Trim();

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

            AssetBundleBuild[] builds = CreateBuilds();

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

        private static AssetBundleBuild[] CreateBuilds()
        {
            var builds = CreateTerrainBuilds();

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var miscAssetsPaths = new List<string>(allAssetPaths.Length - builds.Count);
            var scenePaths = new List<string>();
            var mapInfoPaths = new List<string>();

            var mapInfo = EditorAssets.FindAsset<MapInfo>();

            for (var i = 0; i < allAssetPaths.Length; i++)
            {
                var assetPath = allAssetPaths[i];

                if (!assetPath.StartsWith("Assets/")) continue;
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer is null || importer is MonoImporter || importer is PluginImporter) continue;

                Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (obj is TerrainData) continue;

                if (obj is SceneAsset)
                {
                    scenePaths.Add(assetPath);
                }
                else if (obj is MapInfo ||
                         (mapInfo.LoadingScreenImages != null && mapInfo.LoadingScreenImages.Contains(obj)) ||
                          obj == mapInfo.LoadingScreenMusic ||
                          obj == mapInfo.LoadingScreenLogo)
                {
                    mapInfoPaths.Add(assetPath);
                }
                else
                {
                    miscAssetsPaths.Add(assetPath);
                }

                EditorUtility.DisplayProgressBar("Gathering assets", assetPath, i / (float)allAssetPaths.Length);
            }

            EditorUtility.ClearProgressBar();

            CreateMapInfoBuild(mapInfoPaths, ref builds);
            CreateSceneBuilds(scenePaths, ref builds);
            CreateMiscAssetsBuilds(miscAssetsPaths, ref builds);

            return builds.ToArray();
        }

        private static List<AssetBundleBuild> CreateTerrainBuilds()
        {
            const string progressBarText = "Building terrain scene";
            EditorUtility.DisplayProgressBar(progressBarText, "", 0);
            var terrainScene = EditorSceneManager.GetSceneByPath(Scenes.TERRAIN);

            Terrain[] sortedTerrain = terrainScene.GetAllComponents<Terrain>()
                .Where(terrain => terrain.gameObject.activeInHierarchy)
                .ToArray()
                .Sort();

            var builds = new List<AssetBundleBuild>(sortedTerrain.Length);

            for (int i = 0; i < sortedTerrain.Length; i++)
            {
                string terrainPath = AssetDatabase.GetAssetPath(sortedTerrain[i].terrainData);

                builds.Add(new AssetBundleBuild
                {
                    assetBundleName = $"terraindata_{i}",
                    assetNames = new[] { terrainPath }
                });

                EditorUtility.DisplayProgressBar(progressBarText, terrainPath, i / (float)sortedTerrain.Length);
            }

            EditorUtility.ClearProgressBar();
            return builds;
        }

        private static void CreateMapInfoBuild(List<string> mapInfoAssetPaths, ref List<AssetBundleBuild> builds)
        {
            builds.Add(new AssetBundleBuild
            {
                assetBundleName = Names.MAP_INFO_ASSET_BUNDLE,
                assetNames = mapInfoAssetPaths.ToArray()
            });
        }

        private static void CreateSceneBuilds(List<string> scenePaths, ref List<AssetBundleBuild> builds)
        {
            builds.Add(new AssetBundleBuild {
                assetBundleName = Names.SCENES_ASSET_BUNDLE,
                assetNames = scenePaths.ToArray()
            });
        }

        private static void CreateMiscAssetsBuilds(List<string> assetPaths, ref List<AssetBundleBuild> builds)
        {
            //put big assets in their own assetbundle to avoid the combined assetbundle getting too big.
            //Unity cannot load assetbundles larger then 4GB.
            long assetBundleSize = 0;
            long assetBundleNumber = 1;
            var asssetBundleFiles = new List<string>();

            for (var i = 0; i < assetPaths.Count; i++)
            {
                string absolutePath = Path.GetFullPath(assetPaths[i]);

                //skip directories
                if ((File.GetAttributes(absolutePath) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    continue;
                }

                long fileSize = new FileInfo(absolutePath).Length;

                // if the asset would get too big, create a new assetbundle
                const long maxBundleSize = 500 * 1000000; //500MB
                if (assetBundleSize > 0 && assetBundleSize + fileSize > maxBundleSize)
                {
                    builds.Add(new AssetBundleBuild {
                        assetBundleName = Names.MISC_ASSETS_ASSET_BUNDLES_PREFIX+'_'+assetBundleNumber,
                        assetNames = asssetBundleFiles.ToArray()
                    });

                    assetBundleSize = 0;
                    assetBundleNumber++;
                    asssetBundleFiles = new List<string>();
                }

                asssetBundleFiles.Add(assetPaths[i]);
                assetBundleSize += fileSize;

                //if this is the last asset, create a new assetbundle
                if(assetBundleSize > 0 && i >= assetPaths.Count-1)
                {
                    builds.Add(new AssetBundleBuild {
                        assetBundleName = Names.MISC_ASSETS_ASSET_BUNDLES_PREFIX+'_'+assetBundleNumber,
                        assetNames = asssetBundleFiles.ToArray()
                    });
                }
            }
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
                ManagerVersion = "0.27.13",
                Requirements = new[] { "Mapify" },
                HomePage = mapInfo.homePage,
                Repository = mapInfo.repository
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
