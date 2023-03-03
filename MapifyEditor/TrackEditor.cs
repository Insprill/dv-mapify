using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [CustomEditor(typeof(Track))]
    public class TrackEditor : UnityEditor.Editor
    {
        private Track track;

        public void OnEnable()
        {
            track = (Track)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!track.ParentSwitch && !track.outSwitch)
            {
                if (GUILayout.Button("Add Switch (Left)")) AddSwitch(true);
                if (GUILayout.Button("Add Switch (Right)")) AddSwitch(false);
            }
        }

        private void AddSwitch(bool left)
        {
            Switch sw = Switch.CreateSwitch(track.Curve.Last().position, Quaternion.LookRotation(track.Curve.GetPointAt(1f) - track.Curve.GetPointAt(0.999f)), left);
            sw.isDivergingLeft = left;

            track.outSwitch = sw;

            Undo.RegisterCreatedObjectUndo(sw.gameObject, $"Create {sw.name}");
            sw.gameObject.Select();
        }
    }
}
