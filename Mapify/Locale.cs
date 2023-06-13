using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mapify
{
    public class Locale
    {
        public const string SESSION__MAP_SELECTOR = "mapify/session/map_selector";
        public const string LAUNCHER__SESSION_MAP = "mapify/launcher/session_map";

        private readonly ReadOnlyDictionary<string, Dictionary<string, string>> csv;

        public Locale(ReadOnlyDictionary<string, Dictionary<string, string>> csv)
        {
            this.csv = csv;
        }

        public string Get(string key, string locale = "English")
        {
            if (!csv.ContainsKey(locale))
            {
                Mapify.LogWarning($"Failed to find locale language {locale}");
                return null;
            }

            Dictionary<string, string> localeDict = csv[locale];
            if (localeDict.TryGetValue(key, out string value))
                return value;

            Mapify.LogWarning($"Failed to find locale key {key}");
            return null;
        }
    }
}
