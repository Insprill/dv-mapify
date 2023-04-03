using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(PlayerDistanceGameObjectsDisabler), "OnDisable")]
    public static class PlayerDistanceGameObjectsDisablerPatch
    {
        // Fixes an NRE
        public static bool Prefix(PlayerDistanceGameObjectsDisabler __instance)
        {
            if (__instance.optimizingGameObjects != null)
                return true;
            __instance.StopAllCoroutines();
            return false;
        }
    }
}
