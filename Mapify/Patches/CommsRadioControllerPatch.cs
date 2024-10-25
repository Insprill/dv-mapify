using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV;
using HarmonyLib;
using Mapify.Map;
using Mapify.Components;

namespace Mapify.Patches
{
    /// <summary>
    ///     A temporary patch to remove the Crew Vehicle mode from the comms radio.
    ///     Once garages are implemented, this will only have to be applied if the map doesn't have any.
    /// </summary>
    [HarmonyPatch(typeof(CommsRadioController), nameof(CommsRadioController.Awake))]
    public static class CommsRadioController_Awake_Patch
    {
        public static CommsRadioController controller { get; private set; }

        private static void Postfix(CommsRadioController __instance)
        {
            //TODO why RM crew vehicle?
            // if (!Maps.IsDefaultMap)
            // {
            //     __instance.allModes = __instance.allModes.Where(mode => mode.GetType() != typeof(CommsRadioCrewVehicle)).ToList();
            // }

            // todo Main.LoadedMap.allowTrackBuilding
            modes = modes.Where(mode => mode.GetType() != typeof(CommsRadioCrewVehicle)).ToList();
            controller = __instance;
            CommsRadioController_Field_allModes.SetValue(__instance, modes);
            ___allModes = ___allModes
                .Where(mode => !(mode is CommsRadioCrewVehicle))
                .AddItem(controller.gameObject.AddComponent<CommsRadioTrackBuilder>())
                .ToList();
        }
    }
}
