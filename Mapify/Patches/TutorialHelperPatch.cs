using DV.Utils;
using HarmonyLib;
using Mapify.Map;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(TutorialHelper), "InitializationRoutine")]
    public static class TutorialHelperPatch
    {
        private static bool Prefix()
        {
            if (Maps.IsDefaultMap)
                return true;
            SingletonBehaviour<SaveGameManager>.Instance.data.SetBool("Tutorial_01_completed", true);
            SingletonBehaviour<SaveGameManager>.Instance.data.SetBool("Tutorial_02_completed", true);
            return false;
        }
    }
}
