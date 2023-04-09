using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(StorageAccessPoint), "OnEnable")]
    public static class StorageAccessPoint_OnEnable_Patch
    {
        private static bool Prefix(StorageAccessPoint __instance)
        {
            if (__instance is StorageAccessPointLostAndFound)
                __instance.storage = GameObject.Find("StorageLostAndFound").GetComponent<StorageBase>();
            return true;
        }
    }
}
