using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using HarmonyLib;
using Mapify.Editor;
using UnityModManagerNet;

namespace Mapify
{
    public static class Main
    {
        public static UnityModManager.ModEntry ModEntry;
        public static UnityModManager.ModEntry.ModLogger Logger => ModEntry.Logger;
        public static Settings Settings;
        public static MapInfo MapInfo;
        public static OrderedDictionary MapDirs { get; } = new OrderedDictionary();

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            Settings = Settings.Load<Settings>(modEntry);
            ModEntry.OnGUI = DrawGUI;
            ModEntry.OnSaveGUI = SaveGUI;

            Harmony harmony = null;

            try
            {
                if (!Settings.IsDefaultMap)
                {
                    Logger.Log("Patching...");
                    harmony = new Harmony(ModEntry.Info.Id);
                    harmony.PatchAll();
                    Logger.Log("Successfully patched");
                }
                else
                {
                    Logger.Log("Default map selected, skipping patches");
                }

                DebugCommands.RegisterCommands();

                Logger.Log("Searching for maps...");
                FindMaps();
                Logger.Log($"Found {MapDirs.Count} map(s) {(MapDirs.Count > 0 ? $"({string.Join(", ", MapDirs.Keys.Cast<string>().ToArray())})" : "")}");
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to load {ModEntry.Info.DisplayName}:", ex);
                harmony?.UnpatchAll();
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
            MapDirs.Add("Default", null);
            foreach (string dir in Directory.GetDirectories(ModEntry.Path))
            {
                string[] files = Directory.GetFiles(dir);
                if (files.Any(file => Path.GetFileName(file) == "assets") && files.Any(file => Path.GetFileName(file) == "scenes"))
                    MapDirs.Add(Path.GetFileName(dir), dir);
            }
        }
    }
}
