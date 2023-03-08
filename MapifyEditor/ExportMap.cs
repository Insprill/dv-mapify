using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public class ExportMap : EditorWindow
    {
        private const string STEAM_MOD_PATH = "Steam/steamapps/common/Derail Valley/Mods/Mapify/Map";
        private static ExportMap window;
        private static bool openFolderAfterExport;

        private List<MapValidator.Result> lastResults;

        private static string LastExportPath {
            get => EditorPrefs.GetString("Mapify_LastExportPath");
            set => EditorPrefs.SetString("Mapify_LastExportPath", value);
        }

        // Graphic design is my passion
        private void OnGUI()
        {
            if (lastResults.Count > 0)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label) {
                    richText = true
                };

                GUILayout.Label("Your map has errors!");
                GUILayout.Space(5);

                GUILayout.BeginVertical();
                foreach (MapValidator.Result result in lastResults)
                {
                    GUILayout.Label($"• <color=red>{result.message}</color>", style);
                    GUILayout.Space(5);
                }

                GUILayout.EndVertical();
                GUILayout.Label("Check console for more information.");
                return;
            }

            GUILayout.Label("Your map is ready to export!");

            openFolderAfterExport = GUILayout.Toggle(openFolderAfterExport, "Open Folder After Export");

            if (GUILayout.Button("Export Map"))
            {
                string startingPath;
                string folderName;

                string lastExport = LastExportPath;
                if (!string.IsNullOrEmpty(lastExport) && Directory.Exists(lastExport))
                {
                    startingPath = Path.GetDirectoryName(lastExport);
                    folderName = Path.GetFileName(lastExport.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                }
                else
                {
                    startingPath = GetDefaultSavePath();
                    folderName = "My Custom Map";
                }

                string exportFolderPath = EditorUtility.SaveFolderPanel("Export Map", startingPath, folderName);

                if (!string.IsNullOrWhiteSpace(exportFolderPath))
                {
                    LastExportPath = exportFolderPath;

                    Export(exportFolderPath);

                    if (openFolderAfterExport)
                        EditorUtility.RevealInFinder(exportFolderPath);

                    Close();
                    return;
                }
            }

            if (GUILayout.Button("Close")) Close();
        }

        [MenuItem("Mapify/Export Map")]
        public static void ShowWindow()
        {
            window = GetWindow<ExportMap>();
            window.Show();
            window.titleContent = new GUIContent("Export Map");
            window.lastResults = MapValidator.Validate().ToList().Where(x => x != null).ToList();
            foreach (MapValidator.Result result in window.lastResults) Debug.LogError(result.message, result.context);
        }

        private static void Export(string exportFolderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(exportFolderPath);

            if ((directory.GetFiles().Length > 0 || directory.GetDirectories().Length > 0) && !EditorUtility.DisplayDialog("Clear Folder",
                    "The directory you selected isn't empty, would you like to clear the files from the folder before proceeding? \n \n WARNING: THIS WILL DELETE ALL FILES (EXCLUDING DIRECTORIES) IN THE FOLDER.",
                    "Skip",
                    "Clear Folder"))
                foreach (FileInfo file in directory.GetFiles())
                    file.Delete();

            TrackConnector.ConnectTracks();

            Debug.Log("Building AssetBundles");
            AssetBundleBuild[] builds = {
                new AssetBundleBuild {
                    assetBundleName = "assets",
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle("assets")
                },
                new AssetBundleBuild {
                    assetBundleName = "scenes",
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle("scenes")
                }
            };
            BuildPipeline.BuildAssetBundles(exportFolderPath, builds, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }

        private static string GetDefaultSavePath()
        {
            // search for the user's DV install
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return GetWindowsDefaultSavePath();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return GetLinuxDefaultSavePath();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to find default save path");
                Debug.LogException(e);
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private static string GetWindowsDefaultSavePath()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                string driveRoot = drive.RootDirectory.FullName;
                string potentialPath = Path.Combine(driveRoot, "Program Files", STEAM_MOD_PATH);
                if (Directory.Exists(potentialPath)) return potentialPath;

                potentialPath = Path.Combine(driveRoot, "Program Files (x86)", STEAM_MOD_PATH);
                if (Directory.Exists(potentialPath)) return potentialPath;
            }

            return null;
        }

        private static string GetLinuxDefaultSavePath()
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string potentialPath = Path.Combine(homePath, ".local", "share", STEAM_MOD_PATH);
            return Directory.Exists(potentialPath) ? potentialPath : null;
        }
    }
}
