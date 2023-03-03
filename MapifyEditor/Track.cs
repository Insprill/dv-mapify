using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(BezierCurve))]
    public class Track : MonoBehaviour
    {
        public float age;
        [HideInNormalInspector]
        public Switch inSwitch;
        [HideInNormalInspector]
        public Switch outSwitch;
        [HideInNormalInspector]
        public bool inTrackFirst;
        [HideInNormalInspector]
        public Track inTrack;
        [HideInNormalInspector]
        public bool outTrackFirst;
        [HideInNormalInspector]
        public Track outTrack;
        public BezierCurve Curve => GetComponent<BezierCurve>();
        public Switch ParentSwitch => GetComponentInParent<Switch>();

        private void OnDrawGizmos()
        {
            // Handles.color = Color.red;
            // if (outTrack == null && !outSwitch) DrawDisconnectedIcon(Curve.Last().position);
            // if (inTrack == null && !inSwitch) DrawDisconnectedIcon(Curve[0].position);
        }

        public Transform GetInNodeTransform()
        {
            if (inSwitch) return inSwitch.transform;
            return !inTrack ? null : inTrack.Curve[0].transform;
        }

        public Transform GetOutNodeTransform()
        {
            if (outSwitch) return outSwitch.transform;
            return !outTrack ? null : outTrack.Curve.Last().transform;
        }

        private static void DrawDisconnectedIcon(Vector3 position)
        {
            Handles.Label(position, "Disconnected", EditorStyles.whiteBoldLabel);
            const float size = 0.25f;
            Transform cameraTransform = Camera.current.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraUp = cameraTransform.up;
            Quaternion rotation = Quaternion.LookRotation(cameraForward, cameraUp);
            Handles.DrawLine(position - rotation * Vector3.one * size, position + rotation * Vector3.one * size);
            Handles.DrawLine(position - rotation * new Vector3(size, -size, 0f), position + rotation * new Vector3(size, -size, 0f));
        }

        public void Disconnect()
        {
            inTrack = null;
            outTrack = null;
            inSwitch = null;
            outSwitch = null;
        }

        [MenuItem("Mapify/Create Track")]
        private static void CreateTrack()
        {
            CreateTrack(Vector3.zero, Quaternion.identity).gameObject.Select();
        }

        public static Track CreateTrack(Vector3 position, Quaternion rotation)
        {
            GameObject railwayParent = GameObject.Find("[railway]");
            GameObject trackObject = new GameObject("Track") {
                transform = {
                    parent = railwayParent.transform,
                    position = position,
                    rotation = rotation
                }
            };
            Vector3 dir = trackObject.transform.forward;

            BezierCurve trackCurve = trackObject.AddComponent<BezierCurve>();
            trackCurve.resolution = 0.5f;
            trackCurve.close = false;

            BezierPoint point1 = trackCurve.CreatePointAt(position);
            trackCurve.AddPoint(point1);
            point1.handleStyle = BezierPoint.HandleStyle.Broken;
            point1.handle2 = dir;

            BezierPoint point2 = trackCurve.CreatePointAt(position + dir * 4);
            trackCurve.AddPoint(point2);
            point2.handleStyle = BezierPoint.HandleStyle.Broken;
            point2.handle2 = -dir;

            Track track = trackObject.AddComponent<Track>();

            Undo.RegisterCreatedObjectUndo(trackObject, $"Create {trackObject.name}");

            return track;
        }
    }
}
