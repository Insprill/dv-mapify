using System.Collections.Generic;
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
    [HarmonyPatch(typeof(ContinueLoadNewControllerSingle), nameof(ContinueLoadNewControllerSingle.RefreshData))]
    public static class ContinueLoadNewControllerSingle_RefreshInterface_Patch
    {
        public static Dictionary<ContinueLoadNewControllerSingle, GameObject> MapSelectors { get; } = new Dictionary<ContinueLoadNewControllerSingle, GameObject>(2);

        private static void Postfix(ContinueLoadNewControllerSingle __instance)
        {
            if (!MapSelectors.TryGetValue(__instance, out GameObject mapSelector))
            {
                mapSelector = CreateSelector(__instance);
                MapSelectors[__instance] = mapSelector;
            }
            mapSelector.SetActive(__instance.CurrentThing?.LatestSave == null);
        }

        private static GameObject CreateSelector(ContinueLoadNewControllerSingle __instance)
        {
            Transform toClone = __instance.transform.FindChildByName("selector and button - session");
            GameObject gameObject = GameObject.Instantiate(toClone.gameObject, toClone.parent);
            gameObject.SetActive(false);
            Object.Destroy(gameObject.FindChildByName("ButtonIcon - session"));
            gameObject.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() - 2);
            Selector selector = gameObject.GetComponentInChildren<Selector>();
            Maps.OnMapsUpdated += () =>
            {
                List<string> mapNames = Maps.AllMapNames.ToList();
                selector.SetValues(mapNames);
                selector.SetSelectedIndex(mapNames.FindIndex(name => name == Maps.DEFAULT_MAP_INFO.name));
            };
            selector.SetValues(Maps.AllMapNames.ToList());
            selector.SelectionChanged += (clickable, index) => { OnSelectorClicked(__instance, index); };
            Localize localize = selector.GetComponentInChildren<Localize>();
            localize.key = Locale.SESSION__MAP_SELECTOR;
            localize.UpdateLocalization();
            return gameObject;
        }

        private static void OnSelectorClicked(ContinueLoadNewControllerSingle __instance, int selectedIndex)
        {
            if (__instance.CurrentThing?.GameData == null)
                return;
            BasicMapInfo basicMapInfo = Maps.FromIndex(selectedIndex);
            __instance.CurrentThing.GameData.SetBasicMapInfo(basicMapInfo);
        }
    }
}
