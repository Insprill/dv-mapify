using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Patches.Mods;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify
{
    public static class Main
    {
        public const string DEFAULT_MAP_NAME = "Default";
        private const string MAPS_FOLDER_NAME = "Maps";

        private static UnityModManager.ModEntry ModEntry;
        public static Settings Settings { get; private set; }
        internal static Harmony Harmony { get; private set; }

        private static readonly UnityModManager.ModEntry PassengerJobs = UnityModManager.FindMod("PassengerJobs");
        private static bool IsPassengerJobsEnabled => PassengerJobs != null && PassengerJobs.Enabled;

        public static MapInfo LoadedMap;
        private static BasicMapInfo basicMapInfo;
        public static string[] AllMapNames { get; private set; }
        private static string MapsFolder;
        private static readonly Dictionary<string, (BasicMapInfo, string)> Maps = new Dictionary<string, (BasicMapInfo, string)>();

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
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
                    Log($"Found {Maps.Count} map(s) {(Maps.Count > 0 ? $"({string.Join(", ", Maps.Keys.ToArray())})" : "")}");
                }

                if (Settings.MapName != DEFAULT_MAP_NAME)
                {
                    Log("Patching...");
                    Harmony = new Harmony(ModEntry.Info.Id);
                    Harmony.PatchAll();
                    Log("Successfully patched");
                    if (IsPassengerJobsEnabled)
                    {
                        Log($"Found {PassengerJobs.Info.DisplayName}, patching...");
                        PassengerJobsPatch.Patch(Harmony);
                        Log("Successfully patched");
                    }
                }
                else
                {
                    Log("Default map selected, skipping patches");
                }

                WorldStreamingInit.LoadingFinished += DebugCommands.RegisterCommands;
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

            if (!Maps.ContainsKey(Settings.MapName))
            {
                LogError($"Failed to find selected map {Settings.MapName}! Is it still installed? Was it renamed?");
                Settings.MapName = DEFAULT_MAP_NAME;
            }
            else
            {
                basicMapInfo = Maps[Settings.MapName].Item1;
            }
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
