using System.Collections.Generic;
using System.Reflection.Emit;
using DV.TerrainSystem;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
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
