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

            GUILayout.Space(10);
            if (!GUILayout.Button("Generate Track Name"))
                return;

            foreach (Track track in tracks)
            {
                Undo.RecordObject(track.gameObject, "Generate Track Name");
                track.name = track.LogicName;
            }
        }
    }
}
#endif
