using System;
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using Mapify.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify.Components
{
    /// <summary>
    /// Moves sideways instead of turning
    /// p.s. I had a fever when I wrote most of this code, bear with me
    /// </summary>
    public class TransferTableRailTrack : TurntableRailTrack
    {
        private const float POSITION_THRESHOLD = TransferTableController.ONE_DEGREE_DISTANCE * ANGLE_THRESHOLD;
        private const float SNAPPING_DISTANCE = TransferTableController.ONE_DEGREE_DISTANCE * SNAPPING_ANGLE_DISTANCE_DEGREES;

        private Dictionary<RailTrack, bool> connectsToFront = new Dictionary<RailTrack, bool>();

        public event TracksUpdatedDelegate TracksUpdated2;

        private BoxCollider frontCollider;
        private BoxCollider rearCollider;
        public float TransferTableWidth;

        private void Start()
        {
            //idk why that collider is there tbh
            Destroy(gameObject.GetComponent<BoxCollider>());
        }

        public void SetupColliders()
        {
            var transfertableTransform = transform.parent;
            frontCollider = transfertableTransform.gameObject.AddComponent<BoxCollider>();
            SetupCollider(ref frontCollider, transfertableTransform, true);
            rearCollider = transfertableTransform.gameObject.AddComponent<BoxCollider>();
            SetupCollider(ref rearCollider, transfertableTransform, false);
        }

        private void SetupCollider(ref BoxCollider boxCollider, Transform transfertableTransform, bool front)
        {
            var trackPoint = front ? Track.curve.First() : Track.curve.Last();
            boxCollider.center = transfertableTransform.InverseTransformPoint(trackPoint.position);
            boxCollider.size = new Vector3(TransferTableWidth, 0.1f, 0.1f);
        }

        public void MoveToTargetPosition(bool forceConnectionRefresh = false)
        {
            var currentYrotation = currentYRotation;
            currentYRotation = targetYRotation;

            var delta = currentYRotation - currentYrotation;
            if (Mathf.Abs(delta) == 0.0)
            {
                if (!forceConnectionRefresh) return;
            }
            else
            {
                var delta3D = transform.right * delta;

                for (var index = 0; index < Track.curve.pointCount; ++index)
                {
                    var bezierPoint = Track.curve[index];
                    bezierPoint.position += delta3D;
                    bezierPoint.handle1 += delta3D;
                    if (bezierPoint.handleStyle != BezierPoint.HandleStyle.Connected)
                        bezierPoint.handle2 = bezierPoint.handle2 += delta3D;
                }

                Track.GetKinkedPointSet().Translate(delta3D);
                Track.TrackPointsUpdated_Invoke();
                visuals.Translate(delta3D, Space.World);
            }

            UpdateTrackConnection();
        }

        //todo can we use a transpiler instead? Only the 2 GetConnectedTrackEndAtPosition lines are changed
        public new void UpdateTrackConnection()
        {
            var connectedTrackChanged = false;

            //this is changed from the original function:
            var connectedTrackEndOnFront = GetConnectedTrackEndAtPosition(currentYRotation, true);

            //connected track has changed?
            if (frontClosest != connectedTrackEndOnFront)
            {
                //was there a connected track?
                if (frontClosest != null)
                {
                    if (frontClosest.isFirst)
                    {
                        frontClosest.track.inBranch = null;
                        frontClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicInTrackConnection();
                    }
                    else
                    {
                        frontClosest.track.outBranch = null;
                        frontClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicOutTrackConnection();
                    }
                }
                frontClosest = connectedTrackEndOnFront;

                //IS there a connected track?
                if (frontClosest != null)
                {
                    var branch = new Junction.Branch(Track, true);
                    if (frontClosest.isFirst)
                    {
                        frontClosest.track.inBranch = branch;
                        frontClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicInTrackConnection();
                    }
                    else
                    {
                        frontClosest.track.outBranch = branch;
                        frontClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicOutTrackConnection();
                    }
                    Track.inBranch = new Junction.Branch(frontClosest.track, frontClosest.isFirst);
                }
                else
                {
                    Track.inBranch = null;
                }

                Track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicInTrackConnection();
                connectedTrackChanged = true;
            }

            //this is changed from the original function:
            var connectedTrackEndOnRear = GetConnectedTrackEndAtPosition(currentYRotation, false);

            if (rearClosest != connectedTrackEndOnRear)
            {
                if (rearClosest != null)
                {
                    if (rearClosest.isFirst)
                    {
                        rearClosest.track.inBranch = null;
                        rearClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicInTrackConnection();
                    }
                    else
                    {
                        rearClosest.track.outBranch = null;
                        rearClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicOutTrackConnection();
                    }
                }
                rearClosest = connectedTrackEndOnRear;

                if (rearClosest != null)
                {
                    var branch = new Junction.Branch(Track, false);
                    if (rearClosest.isFirst)
                    {
                        rearClosest.track.inBranch = branch;
                        rearClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicInTrackConnection();
                    }
                    else
                    {
                        rearClosest.track.outBranch = branch;
                        rearClosest.track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicOutTrackConnection();
                    }
                    Track.outBranch = new Junction.Branch(rearClosest.track, rearClosest.isFirst);
                }
                else
                {
                    Track.outBranch = null;
                }

                Track.GetComponent<RailTrackLogicTrackSwitching>().UpdateLogicOutTrackConnection();
                connectedTrackChanged = true;
            }

            if (!connectedTrackChanged) return;

            //can't call TracksUpdated from here
            var tracksUpdated = TracksUpdated2;
            if (tracksUpdated == null)
                return;
            tracksUpdated(frontClosest?.track, rearClosest?.track);
        }

        private TrackEnd GetConnectedTrackEndAtPosition(float position, bool connectToFront)
        {
            return trackEnds.FirstOrDefault(t =>
                connectsToFront[t.track] == connectToFront
                && PositionsEqual(t.angle, position)
            );
        }

        public static bool PositionsEqual(float positionA, float positionB)
        {
            return Mathf.Abs(positionA - positionB) <= POSITION_THRESHOLD;
        }

        // no nullables in c# 7.3
        private enum NullableBool
        {
            _false,
            _true,
            _null
        }

        public new TrackEnd GetTrackEnd(RailTrack otherRailTrack)
        {
            if(otherRailTrack.isJunctionTrack) return null;

            if (!frontCollider)
            {
                SetupColliders();
            }

            var isFirst = NullableBool._null;
            var thisConnectsToFront = NullableBool._null;

            if (InRange(frontCollider, otherRailTrack.curve.First().position))
            {
                isFirst = NullableBool._true;
                thisConnectsToFront = NullableBool._true;
            }
            else if (InRange(frontCollider, otherRailTrack.curve.Last().position))
            {
                isFirst = NullableBool._false;
                thisConnectsToFront = NullableBool._true;
            }

            else if (InRange(rearCollider, otherRailTrack.curve.First().position))
            {
                isFirst = NullableBool._true;
                thisConnectsToFront = NullableBool._false;
            }
            else if (InRange(rearCollider, otherRailTrack.curve.Last().position))
            {
                isFirst = NullableBool._false;
                thisConnectsToFront = NullableBool._false;
            }

            if (isFirst == NullableBool._null) return null;

            connectsToFront[otherRailTrack] = thisConnectsToFront == NullableBool._true;

            var trackEndPoint = isFirst == NullableBool._true ? otherRailTrack.curve.First() : otherRailTrack.curve.Last();
            var position = GetPositionForTrackEnd(trackEndPoint.position);

            return new TrackEnd
            {
                isFirst = isFirst == NullableBool._true,
                track = otherRailTrack,
                angle = position
            };
        }

        private float GetPositionForTrackEnd(Vector3 trackEndPosition)
        {
            return transform.InverseTransformPoint(trackEndPosition).x;
        }

        private static bool InRange(BoxCollider collider, Vector3 position)
        {
            var closestPoint = collider.bounds.ClosestPoint(position);
            var distanceToSurface = Vector3.Distance(position, closestPoint);
            return distanceToSurface <= SEARCH_RADIUS_ALLOWED_OFFSET;
        }

        public bool IsTrackEndWithinSnappingRange(out float closestSnappingPosition)
        {
            closestSnappingPosition = 99999f;
            var canSnap = false;
            var closestDistance = float.MaxValue;
            for (var index = 0; index < trackEnds.Count; ++index)
            {
                var distanceFromCurrent = Mathf.Abs(trackEnds[index].angle - currentYRotation);

                if (distanceFromCurrent <= SNAPPING_DISTANCE && distanceFromCurrent < (double) closestDistance)
                {
                    canSnap = true;
                    closestDistance = distanceFromCurrent;
                    closestSnappingPosition = trackEnds[index].angle;
                }
            }
            return canSnap;
        }

        //no
        public new float ClosestSnappingAngle()
        {
            throw new NotImplementedException();
        }

        public float PositionRange(float position)
        {
            var halfWidth = frontCollider.size.x / 2f; //todo consider bridge width
            if (position > halfWidth) return halfWidth;
            if (position < -halfWidth) return -halfWidth;
            return position;
        }
    }
}
