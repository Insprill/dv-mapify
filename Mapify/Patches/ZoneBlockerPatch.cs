using HarmonyLib;

namespace Mapify.Patches
{
    /// <summary>
    ///     Allows us to show any license when looking at a ZoneBlocker since
    ///     vanilla hard-codes the strings for the military licenses and locomotives.
    ///     This patch applies an offset to the license enum, making it greater than the InteractionInfoType, so we can decode it later.
    /// </summary>
    /// <seealso cref="InteractionTextControllerNonVr_GetText_Patch" />
    [HarmonyPatch(typeof(ZoneBlocker), nameof(ZoneBlocker.Hover))]
    public static class ZoneBlocker_Hover_Patch
    {
        internal const int OFFSET = short.MaxValue; // Doesn't really matter, just has to be larger than the largest actual enum value

        private static bool Prefix(ZoneBlocker __instance)
        {
            if (!(__instance is StationLicenseZoneBlocker blocker))
                return true;

            blocker.Hovered.Invoke();
            if (!VRManager.IsVREnabled())
                SingletonBehaviour<InteractionTextControllerNonVr>.Instance.DisplayText((InteractionInfoType)blocker.requiredJobLicense + OFFSET);

            return false;
        }
    }
}
