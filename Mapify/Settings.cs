using System;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify
{
    [Serializable]
    public class Settings : UnityModManager.ModSettings
    {
        public bool ShowHiddenSettings;
        public bool VerboseLogging;
        public bool ExtremelyVerboseLogging;

        public void Draw(UnityModManager.ModEntry modEntry)
        {
            #region Hidden Settings

            ShowHiddenSettings = GUILayout.Toggle(ShowHiddenSettings, "Show Hidden Settings");

            #region Verbose Logging

            if (ShowHiddenSettings)
            {
                VerboseLogging = GUILayout.Toggle(VerboseLogging, "Verbose Logging");
                ExtremelyVerboseLogging = GUILayout.Toggle(ExtremelyVerboseLogging, "Extremely Verbose Logging");
            }

            #endregion

            #endregion
        }

        public void OnChange()
        {
            // yup
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
