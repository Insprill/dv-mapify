using System;
using System.Reflection;
using CommandTerminal;
using Mapify.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify
{
    public static class DebugCommands
    {
        public static void RegisterCommands()
        {
            try
            {
                MethodInfo[] methods = typeof(DebugCommands).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    if (!(Attribute.GetCustomAttribute(method, typeof(RegisterCommandAttribute)) is RegisterCommandAttribute commandAttribute))
                        return;
                    Action<CommandArg[]> proc = (Action<CommandArg[]>)Delegate.CreateDelegate(typeof(Action<CommandArg[]>), method);
                    Terminal.Shell.AddCommand(commandAttribute.Name, proc, commandAttribute.MinArgCount, commandAttribute.MaxArgCount, commandAttribute.Help, commandAttribute.Hint, commandAttribute.Secret);
                    Terminal.Autocomplete.Register(commandAttribute.Name);
                    Main.Logger.Log($"Registered command {commandAttribute.Name}");
                }
            }
            catch (Exception e)
            {
                Main.Logger.LogException("Failed to register debug commands", e);
            }
        }

        [RegisterCommand("mapify.license", Help = "Modifies a licenses", MinArgCount = 0, MaxArgCount = 2)]
        private static void ModifyLicense(CommandArg[] args)
        {
            if (args.Length == 0)
            {
                foreach (GeneralLicenseType gl in Enum.GetValues(typeof(GeneralLicenseType)))
                    LicenseManager.AcquireGeneralLicense(gl);
                foreach (JobLicenses jl in Enum.GetValues(typeof(JobLicenses)))
                    LicenseManager.AcquireJobLicense(jl);
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
                : (isGeneral && !LicenseManager.IsGeneralLicenseAcquired(generalLicense)) || (isJob && !LicenseManager.IsJobLicenseAcquired(jobLicense));
            if (isGeneral)
            {
                if (addLicense)
                    LicenseManager.AcquireGeneralLicense(generalLicense);
                else
                    LicenseManager.RemoveGeneralLicense(generalLicense);
            }
            else
            {
                if (addLicense)
                    LicenseManager.AcquireJobLicense(jobLicense);
                else
                    LicenseManager.RemoveJobLicense(jobLicense);
            }

            LicenseManager.SaveData();
            Debug.Log($"{(addLicense ? "Granted" : "Revoked")} {(isGeneral ? "general" : "job")} license {licenseName}");
        }

        [RegisterCommand("mapify.money", Help = "Sets the amount of money you have", MinArgCount = 0, MaxArgCount = 1)]
        private static void SetMoney(CommandArg[] args)
        {
            SingletonBehaviour<Inventory>.Instance.SetMoney(args.Length == 0 ? double.MaxValue : args[0].Double());
        }

        [RegisterCommand("mapify.stationInfo", Help = "Prints information about all stations", MaxArgCount = 0)]
        private static void PrintStationInfo(CommandArg[] args)
        {
            foreach (StationController station in Object.FindObjectsOfType<StationController>())
            {
                StationInfo stationInfo = station.stationInfo;
                string str = "\n";
                str += $"{stationInfo.Name}\n";
                str += $"+-- Type: '{stationInfo.Type}'\n";
                str += $"+-- Yard ID: '{stationInfo.YardID}'\n";
                str += $"+-- Color: '{ColorUtility.ToHtmlStringRGBA(stationInfo.StationColor)}'";
                Debug.Log(str);
            }
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
    }
}
