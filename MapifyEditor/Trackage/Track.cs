using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(BezierCurve))]
    public class Track : MonoBehaviour
    {
        public const float SNAP_RANGE = 1.0f;
        public const float SNAP_UPDATE_RANGE_SQR = 250000;
        public const float SNAP_RANGE_SQR = SNAP_RANGE * SNAP_RANGE;

        // ReSharper disable MemberCanBePrivate.Global
        public static readonly Color32 COLOR_ROAD = new Color32(255, 255, 255, 255);
        public static readonly Color32 COLOR_STORAGE = new Color32(172, 134, 101, 255);
        public static readonly Color32 COLOR_LOADING = new Color32(0, 0, 128, 255);
        public static readonly Color32 COLOR_IN = new Color32(50, 240, 50, 255);
        public static readonly Color32 COLOR_OUT = new Color32(106, 90, 205, 255);
        public static readonly Color32 COLOR_PARKING = new Color32(200, 235, 0, 255);
        public static readonly Color32 COLOR_PASSENGER_STORAGE = new Color32(0, 100, 100, 255);
        public static readonly Color32 COLOR_PASSENGER_LOADING = new Color32(0, 255, 255, 255);
        // ReSharper restore MemberCanBePrivate.Global

        [Header("Visuals")]
        [Tooltip("The age of the track. Older tracks are rougher and more rusted, newer tracks are smoother and cleaner")]
        public TrackAge age;
        [Tooltip("Whether speed limit, grade, and marker signs should be generated. Only applies to road tracks")]
        public bool generateSigns;
        [Tooltip("Whether ballast is generated for the track. Doesn't apply to switches")]
        public bool generateBallast = true;
        [Tooltip("Whether sleepers and anchors are generated for the track. Doesn't apply to switches")]
        public bool generateSleepers = true;

        [Header("Job Generation")]
        [Tooltip("The ID of the station this track belongs to")]
        public string stationId;
        [Tooltip("The ID of the yard this track belongs to")]
        public char yardId;
        [Tooltip("The numerical ID of this track in it's respective yard")]
        public byte trackId;
        [Tooltip("The purpose of this track")]
        public TrackType trackType;

#if UNITY_EDITOR
        [Header("Editor Visualization")]
        [SerializeField]
        private bool showLoadingGauge;
#endif

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
        public bool IsTurntable => GetComponentInParent<Turntable>() != null;

        public string LogicName =>
            trackType == TrackType.Road
                ? !generateSigns
                    ? $"[#] {name}"
                    : name
                : $"[Y]_[{stationId}]_[{yardId}-{trackId:D2}-{trackType.LetterId()}]";

        private void OnValidate()
        {
            if (!isActiveAndEnabled || IsSwitch || IsTurntable)
                return;
            switch (trackType)
            {
                case TrackType.Road:
                    Curve.drawColor = COLOR_ROAD;
                    break;
                case TrackType.Storage:
                    Curve.drawColor = COLOR_STORAGE;
                    break;
                case TrackType.Loading:
                    Curve.drawColor = COLOR_LOADING;
                    break;
                case TrackType.In:
                    Curve.drawColor = COLOR_IN;
                    break;
                case TrackType.Out:
                    Curve.drawColor = COLOR_OUT;
                    break;
                case TrackType.Parking:
                    Curve.drawColor = COLOR_PARKING;
                    break;
                case TrackType.PassengerStorage:
                    Curve.drawColor = COLOR_PASSENGER_STORAGE;
                    break;
                case TrackType.PassengerLoading:
                    Curve.drawColor = COLOR_PASSENGER_LOADING;
                    break;
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (showLoadingGauge)
                DrawLoadingGauge();
            if (Curve[0].transform.DistToSceneCamera() >= SNAP_UPDATE_RANGE_SQR && Curve.Last().transform.DistToSceneCamera() >= SNAP_UPDATE_RANGE_SQR)
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
            bool isSelected = !IsSwitch && !IsTurntable && (selectedObjects.Contains(gameObject) || selectedObjects.Contains(Curve[0].gameObject) || selectedObjects.Contains(Curve.Last().gameObject));
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

        private void DrawLoadingGauge()
        {
            Gizmos.color = Curve.drawColor;
            MapInfo mapInfo = EditorAssets.FindAsset<MapInfo>();
            for (int i = 0; i < Curve.pointCount - 1; ++i)
            {
                BezierPoint p1 = Curve[i];
                BezierPoint p2 = Curve[i + 1];
                int resolution = BezierCurve.GetNumPoints(p1, p2, Curve.resolution);
                Vector3[] vector3Array = BezierCurve.Interpolate(p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, resolution);
                Vector3 from = vector3Array[0];
                for (int index = 1; index < vector3Array.Length; ++index)
                {
                    Vector3 to = vector3Array[index];
                    Vector3 center = Vector3.Lerp(from, to, 0.5f);
                    center.y += mapInfo.loadingGaugeHeight / 2;
                    Vector3 direction = to - from;
                    Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapInfo.loadingGaugeWidth, mapInfo.loadingGaugeHeight, Mathf.Abs(direction.magnitude)));
                    from = to;
                }
            }
        }

        private void TrySnap(IEnumerable<BezierPoint> snapPoints, bool move, bool first)
        {
            if (first) isInSnapped = false;
            else isOutSnapped = false;

            BezierPoint point = first ? Curve[0] : Curve.Last();
            Vector3 pos = point.transform.position;
            Vector3 closestPos = Vector3.zero;
            float closestDist = float.MaxValue;

            Collider[] colliders = new Collider[1];
            // Turntables will search for track within 0.05m, so set it a little lower to be safe.
            if (!IsSwitch && Physics.OverlapSphereNonAlloc(pos, 0.04f, colliders) != 0)
            {
                Collider collider = colliders[0];
                Track track = collider.GetComponent<Track>();
                if (collider is CapsuleCollider capsule && track != null && track.IsTurntable)
                {
                    Vector3 center = capsule.transform.TransformPoint(capsule.center);
                    closestPos = pos + (Vector3.Distance(pos, center) - capsule.radius) * -(pos - center).normalized;
                    closestPos.y = center.y;
                    closestDist = Vector3.Distance(pos, closestPos);
                }
            }

            if (closestDist >= float.MaxValue)
                foreach (BezierPoint otherBp in snapPoints)
                {
                    if (otherBp.Curve() == point.Curve()) continue;
                    Vector3 otherPos = otherBp.transform.position;
                    float dist = Mathf.Abs(Vector3.Distance(otherPos, pos));
                    if (dist > SNAP_RANGE || dist >= closestDist) continue;
                    if (IsSwitch && otherBp.GetComponentInParent<Track>().IsSwitch) continue;
                    closestPos = otherPos;
                    closestDist = dist;
                }

            if (closestDist >= float.MaxValue)
                return;

            if (first) isInSnapped = true;
            else isOutSnapped = true;
            if (move) point.transform.position = closestPos;
        }
#endif
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
