using DV.Optimizers;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Fixes a NullReferenceException from trying to iterate over optimizingGameObjects when it's null while disabling.
    /// </summary>
    [HarmonyPatch(typeof(PlayerDistanceGameObjectsDisabler), nameof(PlayerDistanceGameObjectsDisabler.OnDisable))]
    public static class PlayerDistanceGameObjectsDisablerPatch
    {
        private static bool Prefix(PlayerDistanceGameObjectsDisabler __instance)
        {
            __instance.StopAllCoroutines();
            if (__instance.optimizingGameObjects == null)
                return false;
            foreach (GameObject gameObject in __instance.optimizingGameObjects)
                if (gameObject != null)
                    gameObject.SetActive(true);
            return false;
        }
    }
}
