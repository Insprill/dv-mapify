#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Track))]
    public class TrackEditor : UnityEditor.Editor
    {
        private Track[] tracks;

        private void OnEnable()
        {
            tracks = target ? new[] { (Track)target } : targets.Cast<Track>().ToArray();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate Track Name"))
                foreach (Track track in tracks)
                {
                    Undo.RecordObject(track.gameObject, "Generate Track Name");
                    track.name = track.LogicName;
                }

            GUILayout.Space(10);
            GUILayout.Label("Editor Visualization", EditorStyles.boldLabel);
            SerializedProperty showLoadingGaugeProp = serializedObject.FindProperty(nameof(Track.showLoadingGauge));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(showLoadingGaugeProp, new GUIContent("Show Loading Gauge"), true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
