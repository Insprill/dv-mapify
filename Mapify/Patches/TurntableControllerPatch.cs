using System.Reflection;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TurntableController), nameof(TurntableController.GetPushingInput))]
    public class TurntableController_GetPushingInput_Patch
    {
        private static readonly FieldInfo TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS = AccessTools.DeclaredField(typeof(TurntableController), "PUSH_HANDLE_HALF_EXTENTS");

        private static bool Prefix(Transform handle)
        {
            if (handle == null)
                return false;
            BoxCollider collider = handle.GetComponent<BoxCollider>();
            if (collider != null)
                TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS.SetValue(null, collider.size.Add(Track.TURNTABLE_SEARCH_RANGE));
            return true;
        }
    }
}
