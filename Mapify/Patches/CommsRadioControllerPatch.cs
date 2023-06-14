using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV;
using HarmonyLib;
using Mapify.Map;

namespace Mapify.Patches
{
    /// <summary>
    ///     A temporary patch to remove the Crew Vehicle mode from the comms radio.
    ///     Once garages are implemented, this will only have to be applied if the map doesn't have any.
    /// </summary>
    [HarmonyPatch(typeof(CommsRadioController), "Awake")]
    public static class CommsRadioController_Awake_Patch
    {
        private static readonly FieldInfo CommsRadioController_Field_allModes = AccessTools.DeclaredField(typeof(CommsRadioController), "allModes");

        private static void Postfix(CommsRadioController __instance)
        {
            if (Maps.IsDefaultMap)
                return;
            List<ICommsRadioMode> modes = (List<ICommsRadioMode>)CommsRadioController_Field_allModes.GetValue(__instance);
            modes = modes.Where(mode => mode.GetType() != typeof(CommsRadioCrewVehicle)).ToList();
            CommsRadioController_Field_allModes.SetValue(__instance, modes);
        }
    }
}
