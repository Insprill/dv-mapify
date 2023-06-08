using System;
using System.Collections.Generic;
using System.Reflection;
using DV.Common;
using DV.UI;
using DV.UI.PresetEditors;
using HarmonyLib;
using Mapify.Utils;

namespace Mapify.Patches
{
    public static class LauncherControllerAccess
    {
        private static readonly Type type = typeof(LauncherController);
        private static readonly MethodInfo Method_KeyValueFormat = AccessTools.DeclaredMethod(type, "KeyValueFormat", new[] { typeof(string), typeof(string) });

        public static string KeyValueFormat(string locKey, string value)
        {
            return (string)Method_KeyValueFormat.Invoke(null, new object[] { locKey, value });
        }
    }

    [HarmonyPatch(typeof(LauncherController), "GetSaveGameDetails")]
    public static class LauncherController_GetSaveGameDetails_Patch
    {
        private static void Postfix(ISaveGame saveGame, ref string __result)
        {
            List<string> strings = new List<string>(__result.Split('\n'));
            strings.Insert(2, LauncherControllerAccess.KeyValueFormat(Locale.LAUNCHER__SESSION_MAP, saveGame.Data.GetBasicMapInfo().mapName));
            __result = string.Join("\n", strings);
        }
    }

    [HarmonyPatch(typeof(LauncherController), "GetStartGameDetails")]
    public static class LauncherController_GetStartGameDetails_Patch
    {
        private static void Postfix(UIStartGameData data, ref string __result)
        {
            List<string> strings = new List<string>(__result.Split('\n'));
            strings.Insert(2, LauncherControllerAccess.KeyValueFormat(Locale.LAUNCHER__SESSION_MAP, data.session.GameData.GetBasicMapInfo().mapName));
            __result = string.Join("\n", strings);
        }
    }
}
