using System.Linq;
using CommsRadioAPI;
using DV;
using HarmonyLib;
using Mapify.CarLabeler;
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
            if (Maps.IsDefaultMap)
                return;
            __instance.allModes = __instance.allModes.Where(mode => mode.GetType() != typeof(CommsRadioCrewVehicle)).ToList();
        }
    }

    /// <summary>
    /// Create the yard car labeler mode in the comms radio
    /// </summary>
    [HarmonyPatch(typeof(CommsRadioController), nameof(CommsRadioController.Start))]
    public static class CommsRadioController_Start_Patch
    {
        private static bool isRadioSetup = false;

        private static void Postfix()
        {
            if (isRadioSetup) return;

            CommsRadioMode.Create(new Start(), laserColor: Color.blue);
            isRadioSetup = true;
        }
    }

    /// <summary>
    /// By default, scrolling down activates the 'up' button and scrolling up activates the 'down' button. This patch corrects this.
    /// </summary>
    [HarmonyPatch(typeof(CommsRadioController), nameof(CommsRadioController.OnScrolled))]
    public static class CommsRadioController_OnScrolled_Patch
    {
        private static bool Prefix(CommsRadioController __instance, ScrollAction direction)
        {
            if (direction.IsPositive())
                __instance.OnActionA();
            else
                __instance.OnActionB();

            return false;
        }
    }
}
