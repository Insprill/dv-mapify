using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Fixes a NullReferenceException from trying to iterate over optimizingGameObjects when it's null while disabling.
    /// </summary>
    [HarmonyPatch(typeof(PlayerDistanceMultipleGameObjectsOptimizer), "OnDisable")]
    public static class PlayerDistanceMultipleGameObjectsOptimizer_OnDisable_Patch
    {
        private static readonly FieldInfo PlayerDistanceMultipleGameObjectsOptimizer_Field_gameObjectsAndScriptsDisabled =
            AccessTools.DeclaredField(typeof(PlayerDistanceMultipleGameObjectsOptimizer), "gameObjectsAndScriptsDisabled");

        private static bool Prefix(PlayerDistanceMultipleGameObjectsOptimizer __instance)
        {
            __instance.StopAllCoroutines();
            if (__instance.gameObjectsToDisable != null)
                foreach (GameObject gameObject in __instance.gameObjectsToDisable)
                    if (gameObject != null)
                        gameObject.SetActive(true);
            if (__instance.scriptsToDisable != null)
                foreach (MonoBehaviour behaviour in __instance.scriptsToDisable)
                    if (behaviour != null)
                        behaviour.enabled = true;
            PlayerDistanceMultipleGameObjectsOptimizer_Field_gameObjectsAndScriptsDisabled.SetValue(__instance, false);
            return false;
        }
    }
}
