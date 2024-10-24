using System.Collections.Generic;
using System.Reflection.Emit;
using DV.TerrainSystem;
using HarmonyLib;

namespace Mapify.Patches
{
    /// <summary>
    ///     Unity can't find the "Nature/Terrain/Standard" shader when loading, so we replace it with the "Standard" shader.
    ///     This material gets replaced anyways, so this just prevents an exception.
    /// </summary>
    [HarmonyPatch(typeof(TerrainGrid), nameof(TerrainGrid.Awake))]
    public static class TerrainGrid_Awake_Patch
    {
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
