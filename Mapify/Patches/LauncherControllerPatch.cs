using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.Common;
using DV.UI;
using DV.UI.PresetEditors;
using DV.UIFramework;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Map;
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

    [HarmonyPatch(typeof(LauncherController), "OnRunClicked")]
    public static class LauncherController_OnRunClicked_Patch
    {
        private static bool Prefix(LauncherController __instance, ISaveGame ___saveGame)
        {
            if (___saveGame == null)
                return true;

            BasicMapInfo basicMapInfo = ___saveGame.Data.GetBasicMapInfo();
            if (Maps.AllMapNames.Contains(basicMapInfo.mapName))
                return true;

            PopupManager popupManager = null;
            __instance.FindPopupManager(ref popupManager);

            if (!popupManager.CanShowPopup())
            {
                Mapify.LogError("Cannot show popup!");
                return false;
            }

            Popup okPopupPrefab = __instance.GetComponentInParent<InitialScreenController>().continueLoadNewController.career.okPopupPrefab;

            Popup popup = popupManager.ShowPopup(okPopupPrefab);
            popup.labelTMPro.text = Locale.Get(Locale.LAUNCHER__SESSION_MAP_NOT_INSTALLED, basicMapInfo.mapName);
            return false;
        }
    }
}
