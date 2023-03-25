using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Mapify
{
    public class Settings : UnityModManager.ModSettings
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public int MapIndex;
        public bool IsDefaultMap => MapIndex == 0;
        public string MapName => Main.MapDirs.Keys.Cast<string>().ToArray()[Main.Settings.MapIndex]; // 💀
        public string MapDir => (string)Main.MapDirs[MapIndex];

        public void Draw(UnityModManager.ModEntry modEntry)
        {
            // Map Name
            GUILayout.BeginHorizontal();
            GUILayout.Label("Map");
            UnityModManager.UI.PopupToggleGroup(ref MapIndex, Main.MapDirs.Keys.Cast<string>().ToArray());
            GUILayout.EndHorizontal();
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
