using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Mapify.Patches
{
    //The base game has a limit of 30 job generation attempts per station. These patches remove this limitation and instead base the attempts count on the stations job capacity.

    [HarmonyPatch(typeof(StationProceduralJobsController), nameof(StationProceduralJobsController.GenerateProceduralJobsCoro))]
    public static class StationProceduralJobsController_Awake_Patch
    {
        private static readonly FieldInfo field_generationRuleset = typeof(StationProceduralJobsController).GetField(nameof(StationProceduralJobsController.generationRuleset));
        private static readonly FieldInfo field_jobsCapacity = typeof(StationProceduralJobsRuleset).GetField(nameof(StationProceduralJobsRuleset.jobsCapacity));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_I4_S && (byte)code.operand == 30) // generateJobsAttempts < 30
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, field_generationRuleset);
                    yield return new CodeInstruction(OpCodes.Ldfld, field_jobsCapacity);
                    continue;
                }
                if (code.opcode == OpCodes.Ldc_I4_S && (byte)code.operand == 10) // generateJobsAttempts > 10
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, field_generationRuleset);
                    yield return new CodeInstruction(OpCodes.Ldfld, field_jobsCapacity);
                    yield return new CodeInstruction(OpCodes.Div, 3);
                    continue;
                }
                yield return code;
            }
        }
    }
}
