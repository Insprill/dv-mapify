using System.Reflection;
using HarmonyLib;
using Mapify.Components;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TurntableController), nameof(TurntableController.GetPushingInput))]
    public class TurntableController_GetPushingInput_Patch
    {
        private static readonly FieldInfo TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS = AccessTools.DeclaredField(typeof(TurntableController), nameof(TurntableController.PUSH_HANDLE_HALF_EXTENTS));

        private static bool Prefix(Transform handle)
        {
            if (handle == null)
                return false;
            var collider = handle.GetComponent<BoxCollider>();
            if (collider != null)
                TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS.SetValue(null, collider.size.Add(0.05f));
            return true;
        }
    }

    /// <summary>
    /// transfer tables
    /// </summary>
    [HarmonyPatch(typeof(TurntableController), nameof(TurntableController.FixedUpdate))]
    public class TurntableController_FixedUpdate_Patch
    {
        public static bool Prefix(TurntableController __instance)
        {
            if (!(__instance is TransferTableController transferTableController)) return true;
            transferTableController.FixedUpdate();
            return false;
        }
    }
}
