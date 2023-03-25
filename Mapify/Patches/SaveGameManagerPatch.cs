using System.Collections.Generic;
using System.Reflection;
using DV.CashRegister;
using HarmonyLib;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.GetSavePath))]
    public static class SaveGameManager_GetSavePath_Patch
    {
        private static readonly PropertyInfo Property_SaveDirPath = AccessTools.DeclaredProperty(typeof(SaveGameManager), "SaveDirPath");
        private const string SAVEGAME_BASE_NAME = "savegame";

        public static bool Prefix(SaveGameManager __instance, ref string __result)
        {
            string saveName = $"{SAVEGAME_BASE_NAME}-{Main.MapInfo.mapName}";
            __result = string.Join("/", Property_SaveDirPath.GetValue(__instance), __instance.useEncryption ? saveName : $"{saveName}.json");
            return false;
        }
    }

    [HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.Load))]
    public static class SaveGameManager_Load_Postfix_Patch
    {
        public static void Postfix()
        {
            // Skip the tutorial since I'm sure it goes up in flames with a custom map
            SaveGameManager.data.SetBool("Tutorial_01_completed", true);
            SaveGameManager.data.SetBool("Tutorial_02_completed", true);
            SingletonBehaviour<SaveGameManager>.Instance.disableAutosave = false; // The tutorial normally enables this

            // This is normally set by the first one to load, but shops aren't implemented yet so it stays null and causes issues saving.
            CashRegisterBase.allCashRegisters = new List<CashRegisterBase>();
        }
    }
}
