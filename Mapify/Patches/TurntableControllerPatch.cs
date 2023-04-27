using System.Reflection;
using HarmonyLib;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TurntableController), "GetPushingInput")]
    public class TurntableController_GetPushingInput_Patch
    {
        private static readonly FieldInfo TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS = AccessTools.DeclaredField(typeof(TurntableController), "PUSH_HANDLE_HALF_EXTENTS");

        private static bool Prefix(Transform handle)
        {
            if (handle == null)
                return false;
            BoxCollider collider = handle.GetComponent<BoxCollider>();
            if (collider != null)
                TurntableController_Field_PUSH_HANDLE_HALF_EXTENTS.SetValue(null, collider.size.Add(0.05f));
            return true;
        }
    }
}
