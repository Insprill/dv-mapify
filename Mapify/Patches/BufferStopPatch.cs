using DV.TerrainSystem;
using DV.Utils;
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
        private static bool Prefix(BufferStop __instance, Collider other)
        {
            if(Maps.IsDefaultMap) return true; //execute original

            if (TutorialHelper.InRestrictedMode) return false;

            // break velocity
            var breakVelocitySqr = __instance.GetComponent<Editor.BufferStop>().breakSpeed * 3.6f;

            var attachedRigidbody = other.attachedRigidbody;
            if ((attachedRigidbody != null ? !attachedRigidbody.TryGetComponent(out TrainCar _) ? 1 : 0 : 1) != 0 || attachedRigidbody.velocity.sqrMagnitude <= (double) breakVelocitySqr) return false;
            Object.Destroy(__instance.triggerCollider);
            __instance.rb = __instance.gameObject.AddComponent<Rigidbody>();

            // mass after break
            __instance.rb.mass = __instance.GetComponent<Editor.BufferStop>().massAfterBreak;

            if (!(bool) (Object) SingletonBehaviour<TerrainGrid>.Instance) return false;
            __instance.OnTerrainsMove();
            SingletonBehaviour<TerrainGrid>.Instance.TerrainsMoved += __instance.OnTerrainsMove;

            return false; // skip original
        }
    }
}
