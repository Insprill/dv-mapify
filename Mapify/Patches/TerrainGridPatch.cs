using System.Collections.Generic;
using System.Reflection.Emit;
using DV.TerrainSystem;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Sets the proper Material and Basemap Distance on newly generated terrain.
    /// </summary>
    [HarmonyPatch(typeof(TerrainGrid), "Awake")]
    public static class TerrainGrid_Awake_Patch
    {
        private static void Postfix(TerrainGrid __instance)
        {
            foreach (GameObject obj in __instance.generatedTerrains)
            {
                Terrain terrain = obj.GetComponent<Terrain>();
                terrain.materialTemplate = Main.LoadedMap.terrainMaterial;
                terrain.basemapDistance = Main.LoadedMap.terrainBasemapDistance;
            }
        }

        /// <summary>
        ///     Unity can't find the "Nature/Terrain/Standard" shader when loading, so we replace it with the "Standard" shader.
        ///     This material gets replaced anyways, so this just prevents an exception.
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && code.operand as string == "Nature/Terrain/Standard")
                    code.operand = "Standard";
                yield return code;
            }
        }
    }
}
