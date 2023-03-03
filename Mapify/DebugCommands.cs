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
            LicenseManager.AcquireGeneralLicense(GeneralLicenseType.DE6);
            LicenseManager.AcquireGeneralLicense(GeneralLicenseType.SH282);
            LicenseManager.AcquireGeneralLicense(GeneralLicenseType.ConcurrentJobs1);
            LicenseManager.AcquireGeneralLicense(GeneralLicenseType.ConcurrentJobs2);
            LicenseManager.AcquireGeneralLicense(GeneralLicenseType.ManualService);
            LicenseManager.AcquireGeneralLicense(GeneralLicenseType.MultipleUnit);
            LicenseManager.AcquireJobLicense(JobLicenses.Hazmat1);
            LicenseManager.AcquireJobLicense(JobLicenses.Hazmat2);
            LicenseManager.AcquireJobLicense(JobLicenses.Hazmat3);
            LicenseManager.AcquireJobLicense(JobLicenses.Military1);
            LicenseManager.AcquireJobLicense(JobLicenses.Military2);
            LicenseManager.AcquireJobLicense(JobLicenses.Military3);
            LicenseManager.AcquireJobLicense(JobLicenses.Shunting);
            LicenseManager.AcquireJobLicense(JobLicenses.LogisticalHaul);
            LicenseManager.AcquireJobLicense(JobLicenses.TrainLength1);
            LicenseManager.AcquireJobLicense(JobLicenses.TrainLength2);
            Debug.Log("Granted all licenses");
        }
    }
}
