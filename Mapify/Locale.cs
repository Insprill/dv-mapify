using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using I2.Loc;
using Mapify.Utils;

namespace Mapify
{
    public static class Locale
    {
        private const string LOCALE_FILE = "locale.csv";
        private const string DEFAULT_LANGUAGE = "English";
        private const string MISSING_TRANSLATION = "[ MISSING TRANSLATION ]";

        public const string PREFIX = "mapify/";
        private static readonly char[] PREFIX_CHARS = PREFIX.ToCharArray();

        public const string STATION_PREFIX = PREFIX + "station/";
        public const string SESSION__MAP_SELECTOR = PREFIX + "session/map_selector";
        public const string LAUNCHER__SESSION_MAP = PREFIX + "launcher/session_map";
        public const string LAUNCHER__SESSION_MAP_NOT_INSTALLED = PREFIX + "launcher/session_map_not_installed";
        public const string LOADING__PLEASE_WAIT = PREFIX + "loading/please_wait";
        public const string LOADING__LOADING_MAP = PREFIX + "loading/loading_map";

        private static bool initializeAttempted;
        private static ReadOnlyDictionary<string, Dictionary<string, string>> mapifyTranslations;
        private static bool mapSpecificTranslationsLoaded = false;
        private static ReadOnlyDictionary<string, Dictionary<string, string>> mapSpecificTranslations = new (new Dictionary<string, Dictionary<string, string>>());

        public static bool LoadCSV(string modDir)
        {
            initializeAttempted = true;
            var localeFilePath = Path.Combine(modDir, LOCALE_FILE);
            if (!File.Exists(localeFilePath))
            {
                Mapify.LogError($"Failed to find locale file at {localeFilePath}! Please make sure it's there.");
                return false;
            }

            mapifyTranslations = CSV.Parse(File.ReadAllText(localeFilePath));
            return true;
        }

        public static void LoadMapCSV(string mapDir)
        {
            var localeFilePath = Path.Combine(mapDir, LOCALE_FILE);
            if (!File.Exists(localeFilePath))
            {
                Mapify.LogDebug(nameof(LoadMapCSV)+$" no map CSV at {localeFilePath}");
                return;
            }

            mapSpecificTranslations = CSV.Parse(File.ReadAllText(localeFilePath));
            mapSpecificTranslationsLoaded = true;

            Mapify.LogDebug(nameof(LoadMapCSV)+$" loaded map CSV at {localeFilePath}");
        }

        public static void UnloadMapCSV()
        {
            mapSpecificTranslations = new (new Dictionary<string, Dictionary<string, string>>());
            mapSpecificTranslationsLoaded = false;
        }

        public static string Get(string key)
        {
            if (!initializeAttempted)
                throw new InvalidOperationException("Not initialized");

            if (TryGetMapSpecificTranslation(key, out var translation))
            {
                return translation;
            }

            string locale = LocalizationManager.CurrentLanguage;

            if (!mapifyTranslations.ContainsKey(locale))
            {
                if (locale == DEFAULT_LANGUAGE)
                {
                    Mapify.LogError($"Failed to find locale language {locale}! Something is broken, this shouldn't happen. Dumping CSV data:");
                    Mapify.LogError($"\n{CSV.Dump(mapifyTranslations)}");
                    return MISSING_TRANSLATION;
                }

                Mapify.LogWarning($"Failed to find locale language '{locale}', using default language '{DEFAULT_LANGUAGE}'");
                locale = DEFAULT_LANGUAGE;
            }

            Dictionary<string, string> localeDict = mapifyTranslations[locale];

            if (localeDict.TryGetValue(key.TrimStart(PREFIX_CHARS), out string value)) {
                return value;
            }

            // If there is no translation for this station's name, don't translate it.
            if (key.StartsWith(STATION_PREFIX))
            {
                var stationID = key.Replace(STATION_PREFIX, "");
                var stationName = StationController.allStations
                    .Select(stationController => stationController.stationInfo)
                    .Where(stationInfo => stationInfo.YardID == stationID)
                    .Select(stationInfo => stationInfo.Name)
                    .ToList()
                    .FirstOrDefault();
                return stationName;
            }

            return MISSING_TRANSLATION;
        }

        public static bool TryGetMapSpecificTranslation(string key, out string translation)
        {
            translation = "";
            if (!mapSpecificTranslationsLoaded) return false;

            var locale = LocalizationManager.CurrentLanguage;

            if (!mapSpecificTranslations.ContainsKey(locale)) return false;

            var success = mapSpecificTranslations[locale].TryGetValue(key.TrimStart(PREFIX_CHARS), out translation);
            success = success && translation != "";
            return success;
        }

        public static string Get(string key, params object[] placeholders)
        {
            return string.Format(Get(key), placeholders);
        }
    }
}
