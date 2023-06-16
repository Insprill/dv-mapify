#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Station))]
    public class StationEditor : UnityEditor.Editor
    {
        private Station station;

        private void OnEnable()
        {
            station = (Station)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets?.Length != 1) return;
            GUILayout.Space(10);
            GUILayout.Label("Editor Visualization", EditorStyles.boldLabel);
            station.visualizeJobGenerationRange = GUILayout.Toggle(station.visualizeJobGenerationRange, "Visualize Job Generation Range");
            station.visualizeBookletGenerationDistance = GUILayout.Toggle(station.visualizeBookletGenerationDistance, "Visualize Booklet Generation Distance");
            station.visualizeJobDestroyDistance = GUILayout.Toggle(station.visualizeJobDestroyDistance, "Visualize Job Destroy Distance");
        }
    }
}
#endif
