using System.Linq;
using DV.Localization;
using DV.UI;
using DV.UI.PresetEditors;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Map;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(ContinueLoadNewControllerSingle), "Awake")]
    public static class ContinueLoadNewControllerSingle_Awake_Patch
    {
        public static GameObject MapSelector;

        private static void Postfix(ContinueLoadNewControllerSingle __instance)
        {
            Transform toClone = __instance.transform.FindChildByName("selector and button - session");
            GameObject gameObject = GameObject.Instantiate(toClone.gameObject, toClone.parent);
            gameObject.SetActive(false);
            Object.Destroy(gameObject.FindChildByName("ButtonIcon - session"));
            gameObject.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() - 2);
            Selector selector = gameObject.GetComponentInChildren<Selector>();
            selector.SetValues(Maps.AllMapNames.ToList());
            selector.Clicked += _ => { OnSelectorClicked(__instance, selector); };
            Localize localize = selector.GetComponentInChildren<Localize>();
            localize.key = Locale.SESSION__MAP_SELECTOR;
            localize.UpdateLocalization();
            MapSelector = gameObject;
        }

        private static void OnSelectorClicked(ContinueLoadNewControllerSingle __instance, Selector selector)
        {
            if (__instance.CurrentThing?.GameData == null)
                return;
            BasicMapInfo basicMapInfo = Maps.FromIndex(selector.SelectedIndex);
            __instance.CurrentThing.GameData.SetBasicMapInfo(basicMapInfo);
        }
    }

    [HarmonyPatch(typeof(ContinueLoadNewControllerSingle), nameof(ContinueLoadNewControllerSingle.RefreshData))]
    public static class ContinueLoadNewControllerSingle_RefreshInterface_Patch
    {
        private static void Postfix(ContinueLoadNewControllerSingle __instance)
        {
            GameObject mapSelector = ContinueLoadNewControllerSingle_Awake_Patch.MapSelector;
            if (mapSelector == null)
                return;
            mapSelector.SetActive(__instance.CurrentThing?.LatestSave == null);
        }
    }
}
