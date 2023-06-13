using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DV;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;
using UnityModManagerNet;

// using Mapify.Patches.Mods;

namespace Mapify
{
    public static class Mapify
    {
        private static readonly int[] COMPATIBLE_GAME_VERSIONS = { 93 };

        public static readonly BasicMapInfo DEFAULT_MAP_INFO = new BasicMapInfo("Default", null);
        public const string DEFAULT_MAP_NAME = "Default";
        private const string MAPS_FOLDER_NAME = "Maps";
        private const string LOCALE_PATH = "locale.csv";

        private static UnityModManager.ModEntry ModEntry;
        public static Settings Settings { get; private set; }
        internal static Harmony Harmony { get; private set; }

        public static MapInfo LoadedMap;
        private static BasicMapInfo basicMapInfo;
        public static string[] AllMapNames { get; private set; }
        private static string MapsFolder;
        public static readonly Dictionary<string, (BasicMapInfo, string)> Maps = new Dictionary<string, (BasicMapInfo, string)>();
        public static Locale Locale;

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            if (!COMPATIBLE_GAME_VERSIONS.Contains(BuildInfo.BUILD_VERSION_MAJOR))
            {
                LogError($"Incompatible game version {BuildInfo.BUILD_VERSION_MAJOR}! This version of Mapify is only compatible with {string.Join(", ", COMPATIBLE_GAME_VERSIONS)}");
                return false;
            }

            ModEntry = modEntry;
            Settings = Settings.Load<Settings>(modEntry);
            ModEntry.OnGUI = DrawGUI;
            ModEntry.OnSaveGUI = SaveGUI;

            try
            {
                MapsFolder = Path.Combine(ModEntry.Path, MAPS_FOLDER_NAME);
                if (!Directory.Exists(MapsFolder))
                {
                    Directory.CreateDirectory(MapsFolder);
                }
                else
                {
                    Log("Searching for maps...");
                    FindMaps();
                    Log($"Found {Maps.Count} map(s) ({string.Join(", ", Maps.Keys.ToArray())})");
                }

                string localePath = Path.Combine(ModEntry.Path, LOCALE_PATH);
                if (File.Exists(localePath))
                    Locale = new Locale(CSV.Parse(File.ReadAllText(localePath)));
                else
                    LogError($"Failed to find locale file at {localePath}! Please make sure it's there.");

                Log("Patching...");
                Harmony = new Harmony(ModEntry.Info.Id);
                Harmony.PatchAll();
                Log("Successfully patched");
                // if (IsPassengerJobsEnabled)
                // {
                //     Log($"Found {PassengerJobs.Info.DisplayName}, patching...");
                //     PassengerJobsPatch.Patch(Harmony);
                //     Log("Successfully patched");
                // }
            }
            catch (Exception ex)
            {
                LogException($"Failed to load {ModEntry.Info.DisplayName}:", ex);
                Harmony?.UnpatchAll();
                return false;
            }

            return true;
        }

        private static void DrawGUI(UnityModManager.ModEntry entry)
        {
            Settings.Draw(entry);
        }

        private static void SaveGUI(UnityModManager.ModEntry entry)
        {
            Settings.Save(entry);
        }

        private static void FindMaps()
        {
            Maps.Add(DEFAULT_MAP_NAME, (null, null));
            foreach (string dir in Directory.GetDirectories(MapsFolder))
            {
                string mapInfoPath = GetLoadedMapAssetPath("mapInfo.json", dir);
                if (!File.Exists(mapInfoPath)) continue;
                BasicMapInfo mapInfo = JsonUtility.FromJson<BasicMapInfo>(File.ReadAllText(mapInfoPath));
                Maps.Add(mapInfo.mapName, (mapInfo, dir));
            }

            AllMapNames = Maps.Keys.OrderBy(x => x).ToArray();
        }

        public static string GetLoadedMapAssetPath(string fileName, string mapDir = null)
        {
            return Path.Combine(MapsFolder, mapDir ?? Maps[basicMapInfo.mapName].Item2, fileName);
        }

        #region Logging

        public static void LogDebug(object msg)
        {
            if (Settings.VerboseLogging)
                ModEntry.Logger.Log($"[Debug] {msg}");
        }

        public static void Log(object msg)
        {
            ModEntry.Logger.Log($"{msg}");
        }

        public static void LogWarning(object msg)
        {
            ModEntry.Logger.Warning($"{msg}");
        }

        public static void LogError(object msg)
        {
            ModEntry.Logger.Error($"{msg}");
        }

        public static void LogException(object msg, Exception e)
        {
            ModEntry.Logger.LogException($"{msg}", e);
        }

        #endregion
    }
}
