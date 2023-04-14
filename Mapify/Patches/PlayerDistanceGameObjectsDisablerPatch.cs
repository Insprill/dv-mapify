using HarmonyLib;

namespace Mapify.Patches
{
    /// <summary>
    ///     Fixes a NullReferenceException from trying to iterate over optimizingGameObjects when it's null while disabling.
    /// </summary>
    [HarmonyPatch(typeof(PlayerDistanceGameObjectsDisabler), "OnDisable")]
    public static class PlayerDistanceGameObjectsDisablerPatch
    {
        private static bool Prefix(PlayerDistanceGameObjectsDisabler __instance)
        {
            if (__instance.optimizingGameObjects != null)
                return true;
            __instance.StopAllCoroutines();
            return false;
        }
    }
}
