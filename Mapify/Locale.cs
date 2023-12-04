using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using I2.Loc;
using Mapify.Utils;

namespace Mapify
{
    public static class Locale
    {
        private const string DEFAULT_LANGUAGE = "English";
        private const string MISSING_TRANSLATION = "[ MISSING TRANSLATION ]";
        public const string PREFIX = "mapify/";
        public const string STATION_PREFIX = PREFIX + "station/";
        public const string SESSION__MAP_SELECTOR = PREFIX + "session/map_selector";
        public const string LAUNCHER__SESSION_MAP = PREFIX + "launcher/session_map";
        public const string LAUNCHER__SESSION_MAP_NOT_INSTALLED = PREFIX + "launcher/session_map_not_installed";
        public const string LOADING__PLEASE_WAIT = PREFIX + "loading/please_wait";
        public const string LOADING__LOADING_MAP = PREFIX + "loading/loading_map";
        private static readonly char[] PREFIX_CHARS = PREFIX.ToCharArray();

        private static bool initializeAttempted;
        private static ReadOnlyDictionary<string, Dictionary<string, string>> csv;

        public static bool Load(string localeFilePath)
        {
            initializeAttempted = true;
            if (!File.Exists(localeFilePath))
                return false;
            csv = CSV.Parse(File.ReadAllText(localeFilePath));
            return true;
        }

        public static string Get(string key)
        {
            if (!initializeAttempted)
                throw new InvalidOperationException("Not initialized");

            string locale = LocalizationManager.CurrentLanguage;

            if (!csv.ContainsKey(locale))
            {
                if (locale == DEFAULT_LANGUAGE)
                {
                    Mapify.LogError($"Failed to find locale language {locale}! Something is broken, this shouldn't happen. Dumping CSV data:");
                    Mapify.LogError($"\n{CSV.Dump(csv)}");
                    return MISSING_TRANSLATION;
                }

                locale = DEFAULT_LANGUAGE;
                Mapify.LogWarning($"Failed to find locale language {locale}");
            }

            Dictionary<string, string> localeDict = csv[locale];

            if (localeDict.TryGetValue(key.TrimStart(PREFIX_CHARS), out string value)) {
                return value;
            }

            // If there is no translation for this station's name, don't translate it.
            if (key.StartsWith(STATION_PREFIX)) {
                return key.TrimStart(STATION_PREFIX.ToCharArray());
            }

            return MISSING_TRANSLATION;
        }

        public static string Get(string key, params object[] placeholders)
        {
            return string.Format(Get(key), placeholders);
        }
    }
}
