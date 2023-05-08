using System.Collections.Generic;
using System.Linq;
using DV;
using HarmonyLib;
using Mapify.Components;

namespace Mapify.Patches
{
    /// <summary>
    ///     A temporary patch to remove the Crew Vehicle mode from the comms radio.
    ///     Once garages are implemented, this will only have to be applied if the map doesn't have any.
    /// </summary>
    [HarmonyPatch(typeof(CommsRadioController), "Awake")]
    public static class CommsRadioController_Awake_Patch
    {
        public static CommsRadioController controller { get; private set; }

        private static void Postfix(CommsRadioController __instance, ref List<ICommsRadioMode> ___allModes)
        {
            // todo Main.LoadedMap.allowTrackBuilding
            controller = __instance;
            ___allModes = ___allModes
                .Where(mode => !(mode is CommsRadioCrewVehicle))
                .AddItem(controller.gameObject.AddComponent<CommsRadioTrackBuilder>())
                .ToList();
        }
    }
}
