using System.Collections;
using AwesomeTechnologies.VegetationSystem;
using HarmonyLib;
using Mapify.Components;
using Mapify.Editor;
using Mapify.Map;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Pauses loading our custom map, and the rest of the game, until it's ready.
    /// </summary>
    /// <seealso cref="DisplayLoadingInfo_Start_Patch" />
    /// <seealso cref="MapLifeCycle.LoadMap" />
    [HarmonyPatch(typeof(WorldStreamingInit), nameof(WorldStreamingInit.Awake))]
    public static class WorldStreamingInit_Awake_Patch
    {
        public static bool CanLoad = false;
        public static bool CanInitialize = false;

        private static bool Prefix(WorldStreamingInit __instance)
        {
            var saveGameManager = SaveGameManager.Instance;
            saveGameManager.FindStartGameData();
            var basicMapInfo = saveGameManager.GetBasicMapInfo();

            if (!basicMapInfo.IsDefault()){
                SetFakeVegetationStudioPrefab(__instance);
            }

            __instance.StartCoroutine(WaitForLoadingScreen(basicMapInfo));

            return false;
        }

        private static void SetFakeVegetationStudioPrefab(WorldStreamingInit wsi)
        {
            GameObject gameObject = new GameObject("Fake VegetationStudioPrefab");
            gameObject.SetActive(false);
            gameObject.AddComponent<DisableOnAwake>();
            gameObject.AddComponent<VegetationSystemPro>();
            wsi.vegetationStudioPrefab = gameObject;
        }

        private static IEnumerator WaitForLoadingScreen(BasicMapInfo basicMapInfo)
        {
            yield return new WaitUntil(() => CanLoad);

            var wsi = WorldStreamingInit.Instance;

            if(basicMapInfo.IsDefault()){
                wsi.StartCoroutine(MapLifeCycle.LoadMap(basicMapInfo));
            }
            else {
                wsi.StartCoroutine(MapLifeCycleDV.FakeLoadMap());
            }

            yield return new WaitUntil(() => CanInitialize);
            wsi.StartCoroutine(nameof(WorldStreamingInit.LoadingRoutine));
        }
    }
}
