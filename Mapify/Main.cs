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
        public static UnityModManager.ModEntry.ModLogger Logger => ModEntry.Logger;
        public static Settings Settings { get; private set; }
        internal static Harmony Harmony { get; private set; }

        public static readonly UnityModManager.ModEntry PassengerJobs = UnityModManager.FindMod("PassengerJobs");
        public static bool IsPassengerJobsEnabled => PassengerJobs != null && PassengerJobs.Enabled;

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
                    Logger.Log("Searching for maps...");
                    FindMaps();
                    Logger.Log($"Found {Maps.Count} map(s) {(Maps.Count > 0 ? $"({string.Join(", ", Maps.Keys.ToArray())})" : "")}");
                }

                if (Settings.MapName != DEFAULT_MAP_NAME)
                {
                    Logger.Log("Patching...");
                    Harmony = new Harmony(ModEntry.Info.Id);
                    Harmony.PatchAll();
                    Logger.Log("Successfully patched");
                    if (IsPassengerJobsEnabled)
                    {
                        Logger.Log($"Found {PassengerJobs.Info.DisplayName}, patching...");
                        PassengerJobsPatch.Patch(Harmony);
                        Logger.Log("Successfully patched");
                    }
                }
                else
                {
                    Logger.Log("Default map selected, skipping patches");
                }

                WorldStreamingInit.LoadingFinished += DebugCommands.RegisterCommands;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to load {ModEntry.Info.DisplayName}:", ex);
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
                Logger.Error($"Failed to find selected map {Settings.MapName}! Is it still installed? Was it renamed?");
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
    }
}
