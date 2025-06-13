using System.IO;
using DVLangHelper.Data;
using DVLangHelper.Runtime;
using UnityModManagerNet;

namespace Mapify
{
    public static class Locale
    {
        private const string LOCALE_FILE = "locale.csv";
        // copied from DV.Localization.LocalizationAPI.Sanitized
        public const string MISSING_TRANSLATION = "[ MISSING TRANSLATION ]";

        public const string SESSION__MAP_SELECTOR = "session/map_selector";
        public const string LAUNCHER__SESSION_MAP = "launcher/session_map";
        public const string LAUNCHER__SESSION_MAP_NOT_INSTALLED = "launcher/session_map_not_installed";
        public const string LOADING__PLEASE_WAIT = "loading/please_wait";
        public const string LOADING__LOADING_MAP = "loading/loading_map";

        private static TranslationInjector translationsInjector;

        public static bool Setup()
        {
            translationsInjector = new TranslationInjector(Mapify.ModEntry.Info.Id);
            return Reset();
        }

        public static bool Reset()
        {
            translationsInjector.ResetData();
            var localeFilePath = Path.Combine(Mapify.ModEntry.Path, LOCALE_FILE);

            if (!File.Exists(localeFilePath))
            {
                Mapify.LogError($"Failed to find locale file at {localeFilePath}! Please make sure it's there.");
                return false;
            }

            translationsInjector.AddTranslationsFromCsv(localeFilePath);
            return true;
        }

        public static void AddTranslation(string key, DVLanguage translationSetLanguage, string translationSetTranslation)
        {
            translationsInjector.AddTranslation(key, translationSetLanguage, translationSetTranslation);
        }
    }
}
