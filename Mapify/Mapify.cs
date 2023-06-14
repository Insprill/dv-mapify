using System;
using System.IO;
using System.Linq;
using DV;
using HarmonyLib;
using Mapify.Map;
using UnityModManagerNet;

namespace Mapify
{
    public static class Mapify
    {
        private static readonly int[] COMPATIBLE_GAME_VERSIONS = { 93 };

        private const string LOCALE_FILE = "locale.csv";

        private static UnityModManager.ModEntry ModEntry;
        private static Settings Settings { get; set; }
        internal static Harmony Harmony { get; private set; }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;

            if (!IsGameVersionCompatible())
                return false;

            Settings = Settings.Load<Settings>(modEntry);
            ModEntry.OnGUI = entry => Settings.Draw(entry);
            ModEntry.OnSaveGUI = entry => Settings.Save(entry);

            try
            {
                LoadLocale();
                Maps.LoadMaps(ModEntry);
                Patch();
            }
            catch (Exception ex)
            {
                LogException($"Failed to load {ModEntry.Info.DisplayName}", ex);
                Harmony?.UnpatchAll();
                return false;
            }

            return true;
        }

        private static bool IsGameVersionCompatible()
        {
            if (COMPATIBLE_GAME_VERSIONS.Contains(BuildInfo.BUILD_VERSION_MAJOR))
                return true;
            LogError($"Incompatible game version {BuildInfo.BUILD_VERSION_MAJOR}! This version of Mapify is only compatible with {string.Join(", ", COMPATIBLE_GAME_VERSIONS)}");
            return false;
        }

        private static void LoadLocale()
        {
            string localePath = Path.Combine(ModEntry.Path, LOCALE_FILE);
            if (!Locale.Load(localePath))
                LogError($"Failed to find locale file at {localePath}! Please make sure it's there.");
        }

        private static void Patch()
        {
            Log("Patching...");
            Harmony = new Harmony(ModEntry.Info.Id);
            Harmony.PatchAll();
            Log("Successfully patched");
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
