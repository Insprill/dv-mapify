using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(DisplayLoadingInfo), "Start")]
    public static class DisplayLoadingInfo_Start_Patch
    {
        private static void Postfix()
        {
            WorldStreamingInit_Awake_Patch.CanLoad = true;
        }
    }
}
