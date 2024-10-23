using System;
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mapify.Editor
{
    [ExecuteInEditMode] //this is necessary for snapping to work
    [RequireComponent(typeof(VanillaObject))]
    public class Switch : MonoBehaviour
    {
        public enum StandSide
        {
            THROUGH,
            DIVERGING
        }

        [Tooltip("Which side of the switch the stand will appear on")]
        public StandSide standSide;
        [Tooltip("Which way the switch should be flipped by default")]
        public StandSide defaultState;

        private enum SwitchPoint
        {
            FIRST, //the point that is shared between the two tracks
            THROUGH,
            DIVERGING
        }

#if UNITY_EDITOR
        private bool snapShouldUpdate = true;

        private Vector3 previousPositionSwitchFirstPoint;
        private Vector3 previousPositionThroughTrackLastPoint;
        private Vector3 previousPositionDivergingTrackLastPoint;


        private SnappedTrack snappedTrackBeforeSwitch;
        //the track connected to the through track
        private SnappedTrack snappedTrackAfterThrough;
        //the track connected to the diverging track
        private SnappedTrack snappedTrackAfterDiverging;
#endif

        public Track ThroughTrack => transform.Find("[track through]").GetComponent<Track>();
        public Track DivergingTrack => transform.Find("[track diverging]").GetComponent<Track>();
        public bool IsLeft => DivergingTrack.Curve.Last().localPosition.x < 0;

#if UNITY_EDITOR

        private void OnEnable()
        {
            snapShouldUpdate = true;
        }

        private void OnDisable()
        {
            UnsnapConnectedTracks();
        }

        private void OnDestroy()
        {
            UnsnapConnectedTracks();
        }

        private void OnDrawGizmos()
        {
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
            var positionSwitchFirstPoint = transform.position;
            var positionThroughTrackLastPoint = ThroughTrack.Curve.Last().position;
            var positionDivergingTrackLastPoint = DivergingTrack.Curve.Last().position;

            if (positionSwitchFirstPoint != previousPositionSwitchFirstPoint ||
                positionThroughTrackLastPoint != previousPositionThroughTrackLastPoint ||
                positionDivergingTrackLastPoint != previousPositionDivergingTrackLastPoint)
            {
                snapShouldUpdate = true;

                previousPositionSwitchFirstPoint = positionSwitchFirstPoint;
                previousPositionThroughTrackLastPoint = positionThroughTrackLastPoint;
                previousPositionDivergingTrackLastPoint = positionDivergingTrackLastPoint;
            }
        }

        private void UnsnapConnectedTracks()
        {
            snappedTrackBeforeSwitch?.UnSnapped();
            snappedTrackAfterThrough?.UnSnapped();
            snappedTrackAfterDiverging?.UnSnapped();
        }

        public void Snap()
        {
            var bezierPoints = FindObjectsOfType<BezierPoint>();
            bool isSelected = Selection.gameObjects.Contains(gameObject);

            TrySnap(bezierPoints, isSelected, SwitchPoint.FIRST);
            TrySnap(bezierPoints, isSelected, SwitchPoint.DIVERGING);
            TrySnap(bezierPoints, isSelected, SwitchPoint.THROUGH);
        }

        private void TrySnap(IEnumerable<BezierPoint> points, bool move, SwitchPoint switchPoint)
        {
            var reference = switchPoint switch
            {
                SwitchPoint.FIRST => transform,
                SwitchPoint.THROUGH => ThroughTrack.Curve.Last().transform,
                SwitchPoint.DIVERGING => DivergingTrack.Curve.Last().transform,
                _ => throw new ArgumentOutOfRangeException(nameof(switchPoint), switchPoint, null)
            };

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
                switch (switchPoint)
                {
                    case SwitchPoint.FIRST:
                        snappedTrackBeforeSwitch = new SnappedTrack(otherTrack, otherSnapPoint);
                        break;
                    case SwitchPoint.THROUGH:
                        snappedTrackAfterThrough = new SnappedTrack(otherTrack, otherSnapPoint);
                        break;
                    case SwitchPoint.DIVERGING:
                        snappedTrackAfterDiverging = new SnappedTrack(otherTrack, otherSnapPoint);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(switchPoint), switchPoint, null);
                }
            }

            // No snap target found
            if (closestDistance >= float.MaxValue)
            {
                switch (switchPoint)
                {
                    case SwitchPoint.FIRST:
                        snappedTrackBeforeSwitch?.UnSnapped();
                        snappedTrackBeforeSwitch = null;
                        break;
                    case SwitchPoint.THROUGH:
                        snappedTrackAfterThrough?.UnSnapped();
                        snappedTrackAfterThrough = null;
                        break;
                    case SwitchPoint.DIVERGING:
                        snappedTrackAfterDiverging?.UnSnapped();
                        snappedTrackAfterDiverging = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(switchPoint), switchPoint, null);
                }

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
