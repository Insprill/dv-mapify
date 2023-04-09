using System;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify
{
    public class Settings : UnityModManager.ModSettings
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public string MapName = Main.DEFAULT_MAP_NAME;

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
