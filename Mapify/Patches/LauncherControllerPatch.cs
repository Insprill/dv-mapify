using System;
using System.Collections.Generic;
using System.Linq;
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
    [HarmonyPatch]
    public static class LauncherControllerAccess
    {
        private static readonly Type type = typeof(LauncherController);

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(LauncherController), nameof(LauncherController.KeyValueFormat))]
        public static string KeyValueFormat(string locKey, string value)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch(typeof(LauncherController), nameof(LauncherController.GetSaveGameDetails))]
    public static class LauncherController_GetSaveGameDetails_Patch
    {
        private static void Postfix(ISaveGame saveGame, ref string __result)
        {
            List<string> strings = new List<string>(__result.Split('\n'));
            strings.Insert(2, LauncherControllerAccess.KeyValueFormat(Locale.LAUNCHER__SESSION_MAP, saveGame.Data.GetBasicMapInfo().name));
            __result = string.Join("\n", strings);
        }
    }

    [HarmonyPatch(typeof(LauncherController), nameof(LauncherController.GetStartGameDetails))]
    public static class LauncherController_GetStartGameDetails_Patch
    {
        private static void Postfix(UIStartGameData data, ref string __result)
        {
            List<string> strings = new List<string>(__result.Split('\n'));
            strings.Insert(2, LauncherControllerAccess.KeyValueFormat(Locale.LAUNCHER__SESSION_MAP, data.session.GameData.GetBasicMapInfo().name));
            __result = string.Join("\n", strings);
        }
    }

    [HarmonyPatch(typeof(LauncherController), nameof(LauncherController.OnRunClicked))]
    public static class LauncherController_OnRunClicked_Patch
    {
        private static bool Prefix(LauncherController __instance, ISaveGame ___saveGame)
        {
            if (___saveGame == null)
                return true;

            BasicMapInfo basicMapInfo = ___saveGame.Data.GetBasicMapInfo();
            if (Maps.AllMapNames.Contains(basicMapInfo.name))
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
            popup.labelTMPro.text = Locale.Get(Locale.LAUNCHER__SESSION_MAP_NOT_INSTALLED, basicMapInfo.name);
            return false;
        }
    }
}
