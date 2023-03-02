using HarmonyLib;

namespace Mapify.Patches
{
    // Skip the tutorial since I'm sure it goes up in flames with a custom map
    [HarmonyPatch(typeof(SaveGameManager), "DoLoadIO")]
    public static class SaveGameManager_DoLoadIO_Patch
    {
        public static void Postfix()
        {
            SaveGameManager.data.SetBool("Tutorial_01_completed", true);
            SaveGameManager.data.SetBool("Tutorial_02_completed", true);
        }
    }
}
