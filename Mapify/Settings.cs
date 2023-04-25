using System;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify
{
    public class Settings : UnityModManager.ModSettings
    {
        // ReSharper disable MemberCanBePrivate.Global
        public string MapName = Main.DEFAULT_MAP_NAME;
        public bool ShowHiddenSettings;
        public bool verboseLogging;
        // ReSharper restore MemberCanBePrivate.Global
        public bool VerboseLogging => verboseLogging && ShowHiddenSettings;

        public void Draw(UnityModManager.ModEntry modEntry)
        {
            #region Map

            GUILayout.BeginHorizontal();
            GUILayout.Label("Map");
            int idx = Array.IndexOf(Main.AllMapNames, MapName);
            UnityModManager.UI.PopupToggleGroup(ref idx, Main.AllMapNames);
            MapName = Main.AllMapNames[idx];
            GUILayout.EndHorizontal();

            #endregion

            #region Hidden Settings

            ShowHiddenSettings = GUILayout.Toggle(ShowHiddenSettings, "Show Hidden Settings");

            #region Verbose Logging

            if (ShowHiddenSettings)
                verboseLogging = GUILayout.Toggle(verboseLogging, "Verbose Logging");

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
