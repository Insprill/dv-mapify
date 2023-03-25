using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using DV.TerrainSystem;
using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TerrainGrid), "Awake")]
    public static class TerrainGrid_Awake_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            foreach (CodeInstruction code in codes)
                // When you aren't using the MicroSplat material, it creates a new one with a shader that doesn't exist, so we just change it to the Standard shader.
                if (code.opcode == OpCodes.Ldstr && code.operand as string == "Nature/Terrain/Standard")
                    code.operand = "Standard";

            return codes.AsEnumerable();
        }
    }
}
