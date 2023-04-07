using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(BezierCurve), typeof(TrackSnappable))]
    public class Track : MonoBehaviour
    {
        public const float SNAP_RANGE = 1.0f;
        public const float SNAP_UPDATE_RANGE = 500f;

        public float age;
        public string stationId;
        public char yardId;
        public byte trackId;
        public TrackType trackType;
        public bool generateSigns;

        public bool isInSnapped { get; private set; }
        public bool isOutSnapped { get; private set; }
        private BezierCurve _curve;

        public BezierCurve Curve {
            get {
                if (_curve != null) return _curve;
                return _curve = GetComponent<BezierCurve>();
            }
        }

        private Switch _parentSwitch;

        private Switch ParentSwitch {
            get {
                if (_parentSwitch) return _parentSwitch;
                return _parentSwitch = GetComponentInParent<Switch>();
            }
        }

        public bool IsSwitch => ParentSwitch != null;

        private void OnDrawGizmos()
        {
            if ((transform.position - Camera.current.transform.position).sqrMagnitude >= SNAP_UPDATE_RANGE * SNAP_UPDATE_RANGE)
                return;
            if (!isInSnapped)
                DrawDisconnectedIcon(Curve[0].position);
            if (!isOutSnapped)
                DrawDisconnectedIcon(Curve.Last().position);
            Snap();
        }

        internal void Snap()
        {
            BezierPoint[] points = FindObjectsOfType<BezierCurve>().SelectMany(curve => new[] { curve[0], curve.Last() }).ToArray();
            GameObject[] selectedObjects = Selection.gameObjects;
            bool isSelected = !IsSwitch && (selectedObjects.Contains(gameObject) || selectedObjects.Contains(Curve[0].gameObject) || selectedObjects.Contains(Curve.Last().gameObject));
            TrySnap(points, isSelected, true);
            TrySnap(points, isSelected, false);
        }

        private static void DrawDisconnectedIcon(Vector3 position)
        {
            Handles.color = Color.red;
            Handles.Label(position, "Disconnected", EditorStyles.whiteBoldLabel);
            const float size = 0.25f;
            Transform cameraTransform = Camera.current.transform;
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraUp = cameraTransform.up;
            Quaternion rotation = Quaternion.LookRotation(cameraForward, cameraUp);
            Handles.DrawLine(position - rotation * Vector3.one * size, position + rotation * Vector3.one * size);
            Handles.DrawLine(position - rotation * new Vector3(size, -size, 0f), position + rotation * new Vector3(size, -size, 0f));
        }

        private void TrySnap(IEnumerable<BezierPoint> snapPoints, bool move, bool first)
        {
            if (first) isInSnapped = false;
            else isOutSnapped = false;

            BezierPoint point = first ? Curve[0] : Curve.Last();
            Vector3 pos = point.transform.position;
            Vector3 closestPos = Vector3.zero;
            float closestDist = float.MaxValue;
            foreach (BezierPoint otherBp in snapPoints)
            {
                if (otherBp.Curve() == point.Curve()) continue;
                Vector3 otherPos = otherBp.transform.position;
                float dist = Mathf.Abs(Vector3.Distance(otherPos, pos));
                if (dist > SNAP_RANGE || dist >= closestDist) continue;
                closestPos = otherPos;
                closestDist = dist;
            }

            if (closestDist >= float.MaxValue) return;

            if (first) isInSnapped = true;
            else isOutSnapped = true;
            if (move) point.transform.position = closestPos;
        }

        internal void Snapped(BezierPoint point)
        {
            if (point == Curve[0])
                isInSnapped = true;
            if (point == Curve.Last())
                isOutSnapped = true;
        }

        public static Track Find(string stationId, char yardId, byte trackId, TrackType trackType)
        {
            return FindObjectsOfType<Track>().FirstOrDefault(t => t.stationId == stationId && t.yardId == yardId && t.trackId == trackId && t.trackType == trackType);
        }
    }
}
