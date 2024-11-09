using System;
using System.IO;
using HarmonyLib;
using Mapify.BuildMode;
using Mapify.Map;
using RuntimeHandle;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify
{
    public static class Mapify
    {
        public static UnityModManager.ModEntry ModEntry { get; set; }
        public static Settings MySettings { get; private set; }
        private const string LOCALE_FILE = "locale.csv";
        private static AssetBundle assetBundle;

        internal static Harmony Harmony { get; private set; }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;

            MySettings = Settings.Load<Settings>(ModEntry);
            ModEntry.OnGUI = entry => MySettings.Draw(entry);
            ModEntry.OnSaveGUI = entry => MySettings.Save(entry);
            ModEntry.OnUnload = OnUnload;

            try
            {
                LoadLocale();
                Maps.Init();
                LoadAssetBundles();
                Patch();
            }
            catch (Exception ex)
            {
                LogException("Failed to load", ex);
                Harmony?.UnpatchAll(ModEntry.Info.Id);
                return false;
            }

            return true;
        }

        private static void LoadAssetBundles()
        {
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(ModEntry.Path, Editor.Names.ASSET_BUNDLE_NAME));

            if (assetBundle == null)
            {
                throw new Exception("Failed to load asset bundle");
            }

            var assetsFolderPath = Editor.Names.ASSETS_FOLDER_PATH.ToLower();
            BuildModeClass.LoadAssets(assetBundle, assetsFolderPath);
            RuntimeTransformHandle.LoadAssets(assetBundle, assetsFolderPath);
        }

        private static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            Harmony?.UnpatchAll(ModEntry.Info.Id);
            assetBundle?.Unload(true);
            return true;
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

        public static void LogDebugExtreme(Func<object> resolver)
        {
            if (MySettings.ExtremelyVerboseLogging)
                LogDebug(resolver);
        }

        public static void LogDebug(Func<object> resolver)
        {
            if (MySettings.VerboseLogging)
                ModEntry.Logger.Log($"[Debug] {resolver.Invoke()}");
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
