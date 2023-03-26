using System;
using CommandTerminal;
using UnityEngine;

namespace Mapify
{
    public static class DebugCommands
    {
        public static void RegisterCommands()
        {
            try
            {
                Terminal.Shell.AddCommand("mapify.licenses", GrantAllLicenses, help: "Grants all licenses");
                Terminal.Autocomplete.Register("mapify.licenses");
            }
            catch (Exception e)
            {
                Main.Logger.LogException("Failed to register debug commands", e);
            }
        }

        private static void GrantAllLicenses(CommandArg[] args)
        {
            foreach (GeneralLicenseType generalLicense in Enum.GetValues(typeof(GeneralLicenseType)))
                LicenseManager.AcquireGeneralLicense(generalLicense);
            foreach (JobLicenses jobLicense in Enum.GetValues(typeof(JobLicenses)))
                LicenseManager.AcquireJobLicense(jobLicense);
            Debug.Log("Granted all licenses");
        }
    }
}
