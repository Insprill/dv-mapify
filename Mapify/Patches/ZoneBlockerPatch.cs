using HarmonyLib;

namespace Mapify.Patches
{
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
