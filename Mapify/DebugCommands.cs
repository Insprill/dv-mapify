using System;
using CommandTerminal;
using DV.InventorySystem;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using Mapify.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify
{
    public static class DebugCommands
    {
        [RegisterCommand("mapify.license", Help = "Modifies a licenses", MinArgCount = 0, MaxArgCount = 2)]
        private static void ModifyLicense(CommandArg[] args)
        {
            LicenseManager lm = LicenseManager.Instance;
            if (args.Length == 0)
            {
                foreach (GeneralLicenseType gl in Enum.GetValues(typeof(GeneralLicenseType)))
                    lm.AcquireGeneralLicense(gl.ToV2());
                foreach (JobLicenses jl in Enum.GetValues(typeof(JobLicenses)))
                    lm.AcquireJobLicense(jl.ToV2());
                Debug.Log("Granted all licenses");
                return;
            }

            string licenseName = args[0].String;
            bool isGeneral = Enum.TryParse(licenseName, out GeneralLicenseType generalLicense);
            bool isJob = Enum.TryParse(licenseName, out JobLicenses jobLicense);
            if (!isGeneral && !isJob)
            {
                Debug.LogError($"No license found with name '{licenseName}'");
                return;
            }

            bool addLicense = args.Length == 2
                ? args[1].Bool
                : (isGeneral && !lm.IsGeneralLicenseAcquired(generalLicense.ToV2())) || (isJob && !lm.IsJobLicenseAcquired(jobLicense.ToV2()));
            if (isGeneral)
            {
                if (addLicense)
                    lm.AcquireGeneralLicense(generalLicense.ToV2());
                else
                    lm.RemoveGeneralLicense(generalLicense.ToV2());
            }
            else
            {
                if (addLicense)
                    lm.AcquireJobLicense(jobLicense.ToV2());
                else
                    lm.RemoveJobLicense(new[] { jobLicense.ToV2() });
            }

            lm.SaveData(SaveGameManager.Instance.data);
            Debug.Log($"{(addLicense ? "Granted" : "Revoked")} {(isGeneral ? "general" : "job")} license {licenseName}");
        }

        [RegisterCommand("mapify.money", Help = "Sets the amount of money you have", MinArgCount = 0, MaxArgCount = 1)]
        private static void SetMoney(CommandArg[] args)
        {
            // Money is saved as a float, not a double
            SingletonBehaviour<Inventory>.Instance.SetMoney(args.Length == 0 ? float.MaxValue : args[0].Double());
        }

        [RegisterCommand("mapify.spawnItem", Help = "Spawns an item at your feet", MaxArgCount = 1)]
        private static void GiveItem(CommandArg[] args)
        {
            Object obj = Resources.Load(args[0].String);
            GameObject go = obj as GameObject;
            if (go == null)
            {
                Debug.LogError($"Failed to find item {args[0].String}");
                return;
            }

            GameObject instantiated = GameObject.Instantiate(go);
            instantiated.transform.position = PlayerManager.PlayerTransform.position;
        }

        [RegisterCommand("mapify.toggleSaving", Help = "Toggles saving", MaxArgCount = 1)]
        private static void ToggleSaving(CommandArg[] args)
        {
            bool on = args.Length == 0 ? SingletonBehaviour<SaveGameManager>.Instance.disableAutosave : args[0].Bool;
            SingletonBehaviour<SaveGameManager>.Instance.disableAutosave = !on;
            Debug.Log($"{(on ? "Enabled" : "Disabled")} saving");
        }
    }
}
