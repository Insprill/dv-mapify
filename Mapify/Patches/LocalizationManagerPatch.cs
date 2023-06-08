using System;
using System.Reflection;
using DV.Localization;
using HarmonyLib;
using I2.Loc;

namespace Mapify.Patches
{
    public static class LocalizationAPIAccess
    {
        private static readonly Type type = typeof(LocalizationAPI);
        private static readonly MethodInfo Method_ParamGetter = AccessTools.DeclaredMethod(type, "ParamGetter", new[] { typeof(string[]) });
        private static readonly MethodInfo Method_Sanitized = AccessTools.DeclaredMethod(type, "Sanitized", new[] { typeof(string), typeof(string), typeof(string) });

        public static LocalizationManager._GetParam ParamGetter(string[] paramValues)
        {
            return (LocalizationManager._GetParam)Method_ParamGetter.Invoke(null, new object[] { paramValues });
        }

        public static string Sanitized(string key, string translation, string languageOverride = "")
        {
            return (string)Method_Sanitized.Invoke(null, new object[] { key, translation, languageOverride });
        }
    }

    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    public static class LocalizationManager_GetTranslation_Patch
    {
        private const string PREFIX = "mapify/";
        private static readonly char[] PREFIX_CHARS = PREFIX.ToCharArray();

        private static bool Prefix(ref string __result, string Term)
        {
            if (!Term.StartsWith(PREFIX))
                return true;
            string locale = Main.Locale.Get(Term.TrimStart(PREFIX_CHARS));
            __result = LocalizationAPIAccess.Sanitized(Term, locale);
            return false;
        }
    }
}
