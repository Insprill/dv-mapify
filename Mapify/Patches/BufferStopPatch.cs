using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Mapify.Map;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     Allows us to set the break velocity, and mass after break for Buffer Stops.
    /// </summary>
    [HarmonyPatch(typeof(BufferStop), nameof(BufferStop.OnTriggerEnter))]
    public static class BufferStop_OnTriggerEnter_Patch
    {
        /// <summary>
        ///     Replaces the logic removed in the Transpiler.
        /// </summary>
        /// <seealso cref="Transpiler" />
        private static bool Prefix(BufferStop __instance, Collider other)
        {
            Rigidbody attachedRigidbody = other.attachedRigidbody;
            float breakSpeed = Maps.IsDefaultMap
                ? BufferStop.SQR_BREAK_BUFFER_VELOCITY_THRESHOLD
                : __instance.GetComponent<Editor.BufferStop>().breakSpeed * 3.6f;
            return attachedRigidbody != null && attachedRigidbody.velocity.sqrMagnitude <= breakSpeed * breakSpeed;
        }

        /// <summary>
        ///     Replaces the default mass with the mass.
        /// </summary>
        private static void Postfix(BufferStop __instance)
        {
            if (Maps.IsDefaultMap)
                return;
            Rigidbody rigidbody = __instance.gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
                return;
            rigidbody.mass = __instance.GetComponent<Editor.BufferStop>().massAfterBreak;
        }

        /// <summary>
        ///     Removes the velocity check, which is replaced in our Prefix patch.
        /// </summary>
        /// <seealso cref="Prefix" />
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
                if (codes[i].opcode == OpCodes.Ldc_R4 && ((float)codes[i].operand).Eq(BufferStop.SQR_BREAK_BUFFER_VELOCITY_THRESHOLD))
                {
                    for (int j = i - 5; j <= i + 1; j++)
                        codes[j].opcode = OpCodes.Nop;
                    break;
                }

            return codes;
        }
    }
}
