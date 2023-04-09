using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using DV.TerrainSystem;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TerrainGrid), "Awake")]
    public static class TerrainGrid_Awake_Patch
    {
        public static void Postfix(TerrainGrid __instance)
        {
            foreach (GameObject obj in __instance.generatedTerrains)
            {
                Terrain terrain = obj.GetComponent<Terrain>();
                terrain.materialTemplate = Main.LoadedMap.terrainMaterial;
                terrain.basemapDistance = Main.LoadedMap.terrainBasemapDistance;
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            foreach (CodeInstruction code in codes)
                // Unity can't find this shader at runtime, so just substitute it with the standard shader. This doesn't get used anyway so it doesn't matter.
                if (code.opcode == OpCodes.Ldstr && code.operand as string == "Nature/Terrain/Standard")
                    code.operand = "Standard";

            return codes.AsEnumerable();
        }
    }
}
