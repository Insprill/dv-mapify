using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using DV;
using HarmonyLib;
using Mapify.Map;

namespace Mapify
{
    [BepInPlugin("net.insprill.dv-mapify", "Mapify", "0.3.0")]
    [BepInProcess("DerailValley.exe")]
    public class Mapify : BaseUnityPlugin
    {
        public static Mapify Instance { get; private set; }

        private const string LOCALE_FILE = "locale.csv";

        internal Harmony harmony { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Logger.LogFatal("Mapify is already loaded!");
                Destroy(this);
                return;
            }

            Instance = this;

            try
            {
                LoadLocale();
                Maps.LoadMaps();
                Patch();
            }
            catch (Exception ex)
            {
                LogException("Failed to load", ex);
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            Log("Unpatching");
            harmony?.UnpatchSelf();
            Instance = null;
        }

        private void LoadLocale()
        {
            string installDir = Path.GetDirectoryName(Info.Location);
            if (installDir == null)
            {
                Logger.LogError("Failed to find install directory!");
                return;
            }

            string localePath = Path.Combine(installDir, LOCALE_FILE);
            if (!Locale.Load(localePath))
                Logger.LogError($"Failed to find locale file at {localePath}! Please make sure it's there.");
        }

        private void Patch()
        {
            Logger.LogInfo("Patching...");
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo("Successfully patched");
        }

        #region Logging

        public static void LogDebug(object msg)
        {
            Instance.Logger.LogDebug($"{msg}");
        }

        public static void Log(object msg)
        {
            Instance.Logger.LogInfo($"{msg}");
        }

        public static void LogWarning(object msg)
        {
            Instance.Logger.LogWarning($"{msg}");
        }

        public static void LogError(object msg)
        {
            Instance.Logger.LogError($"{msg}");
        }

        public static void LogException(object msg, Exception e, LogLevel level = LogLevel.Error)
        {
            Instance.Logger.Log(level, $"{msg}: {e}");
        }

        #endregion
    }
}
