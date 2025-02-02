using System.Linq;
using DV;
using DV.Utils;
using HarmonyLib;
using Mapify.BuildMode;
using Mapify.Map;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     A temporary patch to remove the Crew Vehicle mode from the comms radio.
    ///     Once garages are implemented, this will only have to be applied if the map doesn't have any.
    /// </summary>
    [HarmonyPatch(typeof(CommsRadioController), nameof(CommsRadioController.Awake))]
    public static class CommsRadioController_Awake_Patch
    {
        private static void Postfix(CommsRadioController __instance)
        {
            //ensure it exists
            var obj2 = SingletonBehaviour<BuildModeClass>.Instance;

            if (Maps.IsDefaultMap)
                return;
            __instance.allModes = __instance.allModes.Where(mode => mode.GetType() != typeof(CommsRadioCrewVehicle)).ToList();
        }
    }
}
