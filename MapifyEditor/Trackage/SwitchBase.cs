using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    [ExecuteInEditMode] //this is necessary for snapping to work
    public abstract class SwitchBase: MonoBehaviour
    {
        public virtual Track[] GetTracks()
        {
            var tracks = gameObject.GetComponentsInChildren<Track>();
            return tracks ?? new Track[] {};
        }

        public abstract BezierPoint GetJointPoint();

        public abstract List<BezierPoint> GetPoints();

#if UNITY_EDITOR
        private bool snapShouldUpdate = true;

        private Vector3[] previousPositionsPoints;
        private SnappedTrack[] snappedTracks;
        private bool init = false;

        private void OnEnable()
        {
            snapShouldUpdate = true;

            var pointsCount = GetTracks().Count()+1;
            previousPositionsPoints = new Vector3[pointsCount];
            snappedTracks = new SnappedTrack[pointsCount];
            init = true;
        }

        private void OnDisable()
        {
            UnsnapConnectedTracks();
            init = false;
        }

        private void OnDestroy()
        {
            UnsnapConnectedTracks();
        }

        private void OnDrawGizmos()
        {
            if(!init) return;
            if (transform.DistToSceneCamera() >= Track.SNAP_UPDATE_RANGE_SQR)
            {
                return;
            }

            CheckSwitchMoved();

            if (snapShouldUpdate)
            {
                Snap();
                snapShouldUpdate = false;
            }
        }

        private void CheckSwitchMoved()
        {
            var positionPoints = GetPoints().Select(point => point.position).ToArray();

            for (int index = 0; index < positionPoints.Length; index++)
            {
                if (positionPoints[index] == previousPositionsPoints[index]) continue;

                snapShouldUpdate = true;
                previousPositionsPoints[index] = positionPoints[index];
            }
        }

        private void UnsnapConnectedTracks()
        {
            foreach (var snapped in snappedTracks)
            {
                snapped?.UnSnapped();
            }
        }

        public void Snap()
        {
            var bezierPoints = FindObjectsOfType<BezierPoint>();
            bool isSelected = Selection.gameObjects.Contains(gameObject);

            var points = GetPoints();
            for (var pointIndex = 0; pointIndex < points.Count; pointIndex++)
            {
                TrySnap(bezierPoints, isSelected, points[pointIndex], pointIndex);
            }
        }

        private void TrySnap(IEnumerable<BezierPoint> points, bool move, BezierPoint snapPoint, int snapPointIndex)
        {
            var reference = snapPoint.transform;

            var position = reference.position;
            var closestPosition = Vector3.zero;
            var closestDistance = float.MaxValue;

            foreach (BezierPoint otherSnapPoint in points)
            {
                //don't connect to itself
                if (otherSnapPoint.Curve().GetComponentInParent<Switch>() == this) continue;

                Vector3 otherPosition = otherSnapPoint.transform.position;
                float distance = Mathf.Abs(Vector3.Distance(otherPosition, position));

                // too far away
                if (distance > Track.SNAP_RANGE || distance >= closestDistance) continue;

                var otherTrack = otherSnapPoint.GetComponentInParent<Track>();

                // don't snap a switch to another switch
                if (otherTrack.IsSwitch) continue;

                closestPosition = otherPosition;
                closestDistance = distance;

                otherTrack.Snapped(otherSnapPoint);

                //remember what track we snapped to
                snappedTracks[snapPointIndex] = new SnappedTrack(otherTrack, otherSnapPoint);
            }

            // No snap target found
            if (closestDistance >= float.MaxValue)
            {
                snappedTracks[snapPointIndex]?.UnSnapped();
                snappedTracks[snapPointIndex] = null;
                return;
            }

            if (move)
            {
                transform.position = closestPosition + (transform.position - reference.position);
            }
        }
#endif
    }
}
