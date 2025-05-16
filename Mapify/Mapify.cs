using System;
using System.IO;
using DV.UI;
using HarmonyLib;
using Mapify.Map;
using Mapify.Patches;
using UnityModManagerNet;
using Object = UnityEngine.Object;

namespace Mapify
{
    public static class Mapify
    {
        private static UnityModManager.ModEntry ModEntry { get; set; }
        private static Settings Settings;

        internal static Harmony Harmony { get; private set; }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;

            Settings = Settings.Load<Settings>(ModEntry);
            ModEntry.OnGUI = entry => Settings.Draw(entry);
            ModEntry.OnSaveGUI = entry => Settings.Save(entry);

            try
            {
                if (!Locale.LoadCSV(ModEntry.Path))
                {
                    return false;
                }
                Maps.Init();
                Patch();
            }
            catch (Exception ex)
            {
                LogException("Failed to load", ex);
                return false;
            }

            return true;
        }

        private static void Patch()
        {
            Log("Patching...");
            Harmony = new Harmony(ModEntry.Info.Id);
            Harmony.PatchAll();
            Log("Successfully patched");
        }

        #region Logging

        public static void LogDebugExtreme(Func<object> resolver)
        {
            if (Settings.ExtremelyVerboseLogging)
                LogDebug(resolver);
        }

        public static void LogDebugExtreme(object msg)
        {
            LogDebugExtreme(() => msg);
        }

        public static void LogDebug(Func<object> resolver)
        {
            if (Settings.VerboseLogging)
                ModEntry.Logger.Log($"[Debug] {resolver.Invoke()}");
        }

        public static void LogDebug(object msg)
        {
            LogDebug(() => msg);
        }

        public static void Log(object msg)
        {
            ModEntry.Logger.Log($"[Info] {msg}");
        }

        public static void LogWarning(object msg)
        {
            ModEntry.Logger.Warning($"{msg}");
        }

        public static void LogError(object msg)
        {
            ModEntry.Logger.Error($"{msg}");
        }

        public static void LogCritical(object msg)
        {
            ModEntry.Logger.Critical($"{msg}");
        }

        public static void LogException(object msg, Exception e)
        {
            ModEntry.Logger.LogException($"{msg}", e);
        }

        #endregion
    }
}
