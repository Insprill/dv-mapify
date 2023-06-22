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
        private const string PREFIX = "mapify/";
        private const string DEFAULT_LANGUAGE = "English";
        public const string SESSION__MAP_SELECTOR = "mapify/session/map_selector";
        public const string LAUNCHER__SESSION_MAP = "mapify/launcher/session_map";
        public const string LAUNCHER__SESSION_MAP_NOT_INSTALLED = "mapify/launcher/session_map_not_installed";
        public const string LOADING__PLEASE_WAIT = "mapify/loading/please_wait";
        public const string LOADING__LOADING_MAP = "mapify/loading/loading_map";
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
                locale = DEFAULT_LANGUAGE;
                Mapify.LogWarning($"Failed to find locale language {locale}");
            }

            Dictionary<string, string> localeDict = csv[locale];
            return localeDict.TryGetValue(key.TrimStart(PREFIX_CHARS), out string value) ? value : "[ MISSING TRANSLATION ]";
        }

        public static string Get(string key, params object[] placeholders)
        {
            return string.Format(Get(key), placeholders);
        }
    }
}
