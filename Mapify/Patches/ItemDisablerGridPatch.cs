using DV;
using DV.TerrainSystem;
using HarmonyLib;
using Mapify.Map;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Replaces the functionality of ItemDisablerGrid#IsSceneAndTerrainLoaded to use our own streamers, ignoring the usesTerrainsAndStreamers flag.
    /// </summary>
    /// <seealso cref="ItemDisablerGrid_Awake_Patch" />
    [HarmonyPatch(typeof(ItemDisablerGrid), nameof(ItemDisablerGrid.IsSceneAndTerrainLoaded))]
    public class ItemDisablerGrid_IsSceneAndTerrainLoaded_Patch
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
