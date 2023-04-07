using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV;
using HarmonyLib;

namespace Mapify.Patches
{
    // This is a temporary patch since the crew vehicle mode is broken
    [HarmonyPatch(typeof(CommsRadioController), "Awake")]
    public static class CommsRadioController_Awake_Patch
    {
        private static readonly FieldInfo CommsRadioController_Field_allModes = AccessTools.DeclaredField(typeof(CommsRadioController), "allModes");

        public static void Postfix(CommsRadioController __instance)
        {
            List<ICommsRadioMode> modes = (List<ICommsRadioMode>)CommsRadioController_Field_allModes.GetValue(__instance);
            modes = modes.Where(mode => mode.GetType() != typeof(CommsRadioCrewVehicle)).ToList();
            CommsRadioController_Field_allModes.SetValue(__instance, modes);
        }
    }
}
