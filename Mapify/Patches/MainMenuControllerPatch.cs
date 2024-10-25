using System.Collections;
using DV.UI;
using DV.UI.PresetEditors;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.Awake))]
    public static class MainMenuController_Awake_Patch
    {
        private static void Postfix(MainMenuController __instance)
        {
            __instance.StartNewGameRequested += OnStartNewGameRequested;
        }

        private static void OnStartNewGameRequested(UIStartGameData data)
        {
            CoroutineManager.Instance.StartCoroutine(DataWaiter());
        }

        private static IEnumerator DataWaiter()
        {
            AStartGameData data;
            while ((data = Object.FindObjectOfType<AStartGameData>()) == null)
                yield return null;
            AUserProfileProvider profileProvider = Object.FindObjectOfType<AUserProfileProvider>();
            BasicMapInfo basicMapInfo = profileProvider.CurrentSession.GameData.GetBasicMapInfo();
            data.GetSaveGameData().SetBasicMapInfo(basicMapInfo);
        }
    }
}
