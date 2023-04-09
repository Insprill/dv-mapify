using System;
using System.Collections.Generic;
using System.IO;
using DV.CashRegister;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.GetSavePath))]
    public static class SaveGameManager_GetSavePath_Patch
    {
        private const string SAVEGAME_BASE_NAME = "savegame";

        internal static readonly string SaveDirPath = (string)AccessTools.DeclaredProperty(typeof(SaveGameManager), "SaveDirPath").GetValue(null);

        public static bool Prefix(SaveGameManager __instance, ref string __result)
        {
            string saveName = GetSavegameName();
            __result = Path.Combine(SaveDirPath, __instance.useEncryption ? saveName : $"{saveName}.json");
            return false;
        }

        internal static string GetSavegameName()
        {
            return $"{SAVEGAME_BASE_NAME}-{Main.LoadedMap.mapName}";
        }
    }

    [HarmonyPatch(typeof(SaveGameManager), "MakeBackupFile")]
    public static class SaveGameManager_MakeBackupFile_Patch
    {
        public static bool Prefix(string filePath)
        {
            string str = Path.Combine(
                SaveGameManager_GetSavePath_Patch.SaveDirPath,
                $"{SaveGameManager_GetSavePath_Patch.GetSavegameName()}_backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
            );
            if (File.Exists(str))
                Debug.LogWarning($"File '{str}' already exists, not creating backup");
            else
                File.Copy(filePath, str);
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
