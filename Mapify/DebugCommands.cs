using System;
using System.Reflection;
using CommandTerminal;
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

        [RegisterCommand("mapify.licenses", Help = "Grants all licenses", MaxArgCount = 0)]
        private static void GrantAllLicenses(CommandArg[] args)
        {
            foreach (GeneralLicenseType generalLicense in Enum.GetValues(typeof(GeneralLicenseType)))
                LicenseManager.AcquireGeneralLicense(generalLicense);
            foreach (JobLicenses jobLicense in Enum.GetValues(typeof(JobLicenses)))
                LicenseManager.AcquireJobLicense(jobLicense);
            Debug.Log("Granted all licenses");
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
    }
}
