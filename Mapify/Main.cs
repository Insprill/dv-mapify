using System;
using HarmonyLib;
using UnityModManagerNet;

namespace Mapify
{
    public static class Main
    {
        public static UnityModManager.ModEntry ModEntry;
        public static UnityModManager.ModEntry.ModLogger Logger => ModEntry.Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;

            Harmony harmony = null;

            try
            {
                Logger.Log("Patching...");
                harmony = new Harmony(ModEntry.Info.Id);
                harmony.PatchAll();
                Logger.Log("Successfully patched");

                DebugCommands.RegisterCommands();
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to load {ModEntry.Info.DisplayName}:", ex);
                harmony?.UnpatchAll();
                return false;
            }

            return true;
        }
    }
}
