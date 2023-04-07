using HarmonyLib;
using Mapify.Utils;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(InteractionTextControllerNonVr), nameof(InteractionTextControllerNonVr.GetText))]
    public class InteractionTextControllerNonVr_GetText_Patch
    {
        public static bool Prefix(InteractionInfoType infoType, ref string __result)
        {
            if ((int)infoType < ZoneBlocker_Hover_Patch.OFFSET)
                return true;

            __result = $"This area\nrequires\nlicense \"{((JobLicenses)infoType - ZoneBlocker_Hover_Patch.OFFSET).ToSpacedString()}\"";

            return false;
        }
    }
}
