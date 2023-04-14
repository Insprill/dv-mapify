using System.Collections.Generic;
using System.Reflection.Emit;
using DV;
using DV.TerrainSystem;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     When initializing, the two default streamers ([near] and [far]) are hard referenced by name.
    ///     If they aren't found, this warning message is printed, and a flag is set so ItemDisablerGrid#IsSceneAndTerrainLoaded always return true.
    ///     This patch removes the warning message, and the other patch replaces the functionality of that method, using our own streamers.
    /// </summary>
    /// <seealso cref="ItemDisablerGrid_IsSceneAndTerrainLoaded_Patch" />
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
