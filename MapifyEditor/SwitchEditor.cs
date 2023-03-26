using Mapify.Editor.Utils;
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

            if ((sw.divergingTrack.outTrack == null && sw.divergingTrack.outSwitch == null) && GUILayout.Button("Add Track From Diverging")) AddTrack(true);
            if ((sw.throughTrack.outTrack == null && sw.throughTrack.outSwitch == null) && GUILayout.Button("Add Track From Through")) AddTrack(false);
        }

        private void AddTrack(bool diverging)
        {
            Track swTrack = diverging ? sw.divergingTrack : sw.throughTrack;
            BezierCurve curve = swTrack.Curve;
            Quaternion rot = Quaternion.LookRotation(curve.GetPointAt(1f) - curve.GetPointAt(0.999f));

            Track track = Track.CreateTrack(curve.Last().position, rot);
            swTrack.outTrack = track;
            track.inSwitch = sw;

            track.gameObject.Select();
        }
    }
}
