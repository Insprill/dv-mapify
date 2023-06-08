using System.Linq;
using DV.JObjectExtstensions;
using DV.Localization;
using DV.UI;
using DV.UI.PresetEditors;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Newtonsoft.Json.Linq;
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
            selector.SetValues(Main.AllMapNames.ToList());
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
            BasicMapInfo basicMapInfo = Main.Maps[Main.AllMapNames[selector.SelectedIndex]].Item1;
            if (basicMapInfo == null)
                __instance.CurrentThing.GameData.Remove("mapify");
            else
                __instance.CurrentThing.GameData.SetJObject("mapify", JObject.FromObject(basicMapInfo));
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
