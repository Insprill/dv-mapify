using DV.UI;
using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.GoBackToMainMenu))]
    public static class MainMenuPatch
    {
        private static void Postfix()
        {
            StreamedObjectInitPatch.ResetStreamers();
        }
    }
}
