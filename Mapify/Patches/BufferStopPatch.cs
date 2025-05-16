using HarmonyLib;
using Mapify.Map;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Allows us to set the break velocity, and mass after break for Buffer Stops.
    /// </summary>
    [HarmonyPatch(typeof(BufferStop), nameof(BufferStop.OnTriggerEnter))]
    public static class BufferStop_OnTriggerEnter_Patch
    {
        private static void Prefix(BufferStop __instance, Collider other)
        {
            if (Maps.IsDefaultMap) return;

            __instance.breakVelocitySqr = __instance.GetComponent<Editor.BufferStop>().breakSpeed * 3.6f;
        }

        private static void Postfix(BufferStop __instance)
        {
            if (Maps.IsDefaultMap) return;

            //the RigidBody is created in OnTriggerEnter so we have to set rb.mass in a Postfix
            __instance.rb.mass = __instance.GetComponent<Editor.BufferStop>().massAfterBreak;
        }
    }
}
