using System.Linq;
using DV;
using DV.InventorySystem;
using DV.Utils;
using HarmonyLib;
using Mapify.Components;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(CommsRadioController), nameof(CommsRadioController.Awake))]
    public static class CommsRadioController_Awake_Patch
    {
        public static CommsRadioController Controller { get; private set; }

        private static void Postfix(CommsRadioController __instance)
        {
            Controller = __instance;

            var builder = __instance.gameObject.AddComponent<CommsRadioTrackBuilder>();
            __instance.allModes = __instance.allModes
                .AddItem(builder)
                .ToList();
        }
    }
}
