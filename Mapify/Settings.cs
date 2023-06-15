using BepInEx.Configuration;

namespace Mapify
{
    public class Settings
    {
        public readonly ConfigEntry<bool> VerboseLogging;

        public Settings(ConfigFile config)
        {
            VerboseLogging = config.Bind("Advanced", "Verbose Logging", false, "Whether to log more information than usual. Only use when debugging problems.");
        }
    }
}
