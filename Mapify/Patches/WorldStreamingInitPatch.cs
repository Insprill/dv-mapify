using System.Collections;
using AwesomeTechnologies.VegetationSystem;
using DV.TerrainSystem;
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
            SaveGameManager saveGameManager = SaveGameManager.Instance;
            saveGameManager.FindStartGameData();
            BasicMapInfo basicMapInfo = saveGameManager.GetBasicMapInfo();
            if (basicMapInfo.IsDefault())
                return true;
            SetFakeVegetationStudioPrefab(__instance);
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
            WorldStreamingInit wsi = WorldStreamingInit.Instance;
            yield return new WaitUntil(() => CanLoad);
            wsi.StartCoroutine(MapLifeCycle.LoadMap(basicMapInfo));
            yield return new WaitUntil(() => CanInitialize);
            wsi.StartCoroutine(nameof(WorldStreamingInit.LoadingRoutine));
        }
    }

    /// <summary>
    ///     Replaces the functionality of WorldStreamingInit#IsSceneAndTerrainRegionLoaded to use our own streamers, ignoring the usesTerrainsAndStreamers flag.
    /// </summary>
    [HarmonyPatch(typeof(WorldStreamingInit), nameof(WorldStreamingInit.IsSceneAndTerrainRegionLoaded))]
    public class WorldStreamingInit_IsSceneAndTerrainRegionLoaded_Patch
    {
        private static Streamer[] streamers;

        private static bool Prefix(Vector3 worldPos, ref bool __result)
        {
            if (Maps.IsDefaultMap)
                return true;

            if (streamers == null)
                streamers = Object.FindObjectsOfType<Streamer>();

            foreach (Streamer streamer in streamers)
            {
                if (streamer.IsSceneLoaded(worldPos)) continue;
                __result = false;
                return false;
            }

            __result = TerrainGrid.Instance.IsInLoadedRegion(worldPos);
            return false;
        }
    }
}
