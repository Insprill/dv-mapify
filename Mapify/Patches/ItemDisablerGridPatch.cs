using System.Collections.Generic;
using System.Reflection.Emit;
using DV;
using DV.TerrainSystem;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(ItemDisablerGrid), "Awake")]
    public class ItemDisablerGrid_Awake_Patch
    {
        //todo: remove the debug log call. it's 5am and i can't get it to work lol
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand as string == "IsSceneAndTerrainLoaded will return true always, because not all of the streamers/terrains exist!")
                    code.operand = string.Empty;

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(ItemDisablerGrid), nameof(ItemDisablerGrid.IsSceneAndTerrainLoaded))]
    public class ItemDisablerGrid_IsSceneAndTerrainLoaded_Patch
    {
        private static Streamer[] streamers;

        private static bool Prefix(Vector3 worldPos, ref bool __result)
        {
            if (streamers == null)
                streamers = Object.FindObjectsOfType<Streamer>();

            foreach (Streamer streamer in streamers)
            {
                if (streamer.IsSceneLoaded(worldPos)) continue;
                __result = false;
                return false;
            }

            __result = SingletonBehaviour<TerrainGrid>.Instance.IsInLoadedRegion(worldPos);
            return false;
        }
    }
}
