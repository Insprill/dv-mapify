using HarmonyLib;
using I2.Loc;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    public static class LocalizationManager_GetTranslation_Patch
    {
        private const string PREFIX = "mapify/";
        private static readonly char[] PREFIX_CHARS = PREFIX.ToCharArray();

        private static bool Prefix(ref string __result, string Term)
        {
            if (!Term.StartsWith(PREFIX))
                return true;
            __result = Locale.Get(Term);
            return false;
        }
    }
}
