using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor
{
    public class TrackSnappable : MonoBehaviour
    {
        [Tooltip("The transform to use as a reference when snapping. Will use self if not set")]
        public Transform referencePoint;
        public bool onlySnapToEnds = true;

        private void OnDrawGizmos()
        {
            if (transform.DistToSceneCamera() >= Track.SNAP_UPDATE_RANGE_SQR)
                return;
            BezierPoint[] snapPoints = FindObjectsOfType<BezierCurve>().SelectMany(curve => onlySnapToEnds ? curve.GetFirstAndLastPoints() : curve.GetAnchorPoints()).ToArray();
            TrySnap(snapPoints);
            TrySnap(snapPoints);
        }

        private void TrySnap(IEnumerable<BezierPoint> snapPoints)
        {
            Vector3 selfPos = transform.position;
            Vector3 pos = referencePoint == null ? selfPos : referencePoint.position;
            BezierPoint closestPoint = null;
            float closestDist = float.MaxValue;
            foreach (BezierPoint otherPoint in snapPoints)
            {
                Vector3 otherPos = otherPoint.transform.position;
                float dist = Mathf.Abs(Vector3.Distance(otherPos, pos));
                if (dist > Track.SNAP_RANGE || dist >= closestDist) continue;
                closestPoint = otherPoint;
                closestDist = dist;
            }

            if (closestDist >= float.MaxValue || closestPoint == null)
                return;

            closestPoint.Curve().GetComponent<Track>().Snapped(closestPoint);
            transform.position = closestPoint.position + (selfPos - pos);
        }
    }
}
