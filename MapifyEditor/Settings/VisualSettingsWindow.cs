using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Mapify.Editor.Tools
{
    //todo save
    public class VisualSettingsWindow: EditorWindow
    {
        [MenuItem("Mapify/Visual settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<VisualSettingsWindow>();
            window.Show();
            window.titleContent = new GUIContent("Visual settings");
            window.autoRepaintOnSceneChange = true;
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            var settings = VisualSettings.Instance;

            settings.EnableTrackVisuals = EditorGUILayout.Toggle("Enable Track Visuals", settings.EnableTrackVisuals);
            settings.TrackPreviewPrefab = EditorHelper.ObjectField(
                new GUIContent("Track prefab", ""),
                settings.TrackPreviewPrefab, false);

            if (EditorGUI.EndChangeCheck())
            {
                OnValueChanged();
            }
        }

        private void OnValueChanged()
        {
            // var settings = VisualSettings.Instance;
            // settings.PrefabSize = TrackPreviewPrefab.GetComponent<MeshRenderer>().bounds.size.z;

            // foreach (var track in FindObjectsOfType<Track>())
            // {
            //     track.VisualCleanup();
            //     track.OnSettingsChanged();
            // }
        }
    }
}

#endif
