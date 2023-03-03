using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [CustomEditor(typeof(Switch))]
    public class SwitchEditor : UnityEditor.Editor
    {
        private Switch sw;

        public void OnEnable()
        {
            sw = (Switch)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (sw.divergingTrack == null && GUILayout.Button("Add Track From Diverging")) AddTrack(true);
            if (sw.throughTrack == null && GUILayout.Button("Add Track From Through")) AddTrack(false);
        }

        private void AddTrack(bool diverging)
        {
            BezierCurve curve = (diverging ? sw.divergingTrack : sw.throughTrack).Curve;
            Quaternion rot = Quaternion.LookRotation(curve.GetPointAt(1f) - curve.GetPointAt(0.999f));

            Track track = Track.CreateTrack(curve.Last().position, rot);
            track.inSwitch = sw;

            Selection.objects = new Object[] { track.gameObject };
        }
    }
}
