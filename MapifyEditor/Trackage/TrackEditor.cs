using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Track))]
    public class TrackEditor : UnityEditor.Editor
    {
        private Track track;

        private void OnEnable()
        {
            track = (Track)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets?.Length != 1) return;
            GUILayout.Space(10);
            GUILayout.Label("Editor Visualization", EditorStyles.boldLabel);
            track.showLoadingGauge = GUILayout.Toggle(track.showLoadingGauge, "Show Loading Gauge");
        }
    }
}
