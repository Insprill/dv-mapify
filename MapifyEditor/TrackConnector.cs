using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public static class TrackConnector
    {
        private const float MAX_CONNECT_RANGE = 1.0f;

        [MenuItem("Mapify/Debug/Connect Tracks")]
        public static void ConnectTracks()
        {
            Debug.Log("Connecting all tracks");
            Track[] tracks = Object.FindObjectsOfType<Track>();
            foreach (Track track in tracks) track.Disconnect();

            foreach (Track track in tracks)
            {
                Switch parentSwitch = track.GetComponentInParent<Switch>();
                if (parentSwitch)
                    switch (track.name)
                    {
                        case "[track through]":
                            parentSwitch.throughTrack = track;
                            track.inSwitch = parentSwitch;
                            break;
                        case "[track diverging]":
                            parentSwitch.divergingTrack = track;
                            track.inSwitch = parentSwitch;
                            break;
                        default:
                            Debug.LogWarning($"Unknown track {track.name} under Switch. Should be '[track through]' or '[track diverging]'", track);
                            break;
                    }

                Switch closestInSwitch = FindClosestJunction(track.Curve[0].position);
                Switch closestOutSwitch = FindClosestJunction(track.Curve.Last().position);
                if (closestInSwitch && !parentSwitch)
                    ConnectInToSwitch(track, closestInSwitch);
                else if (closestOutSwitch && !parentSwitch)
                    ConnectOutToSwitch(track, closestOutSwitch);
            }

            foreach (Track track in tracks)
            {
                ConnectInToClosestBranch(track);
                ConnectOutToClosestBranch(track);
            }
        }

        private static void ConnectInToSwitch(Track track, Switch sw)
        {
            track.inSwitch = sw;
            track.inSwitch.inTrack = track;
            MoveInToConnected(track);
        }

        private static void ConnectOutToSwitch(Track track, Switch sw)
        {
            track.outSwitch = sw;
            track.outSwitch.inTrack = track;
            MoveOutToConnected(track);
        }

        private static void ConnectInToClosestBranch(Track track)
        {
            ConnectToClosestBranch(track, track.Curve[0], true);
        }

        private static void ConnectOutToClosestBranch(Track track)
        {
            ConnectToClosestBranch(track, track.Curve.Last(), false);
        }

        private static void ConnectToClosestBranch(Track track, BezierPoint bezierPoint, bool first)
        {
            if (first ? track.inSwitch : track.outSwitch) return;

            Branch closestBranch = FindClosestBranch(track, bezierPoint.position);
            Branch branch = new Branch(track, first);
            if (closestBranch != null && closestBranch.track)
            {
                if (first)
                {
                    track.inTrackFirst = closestBranch.first;
                    track.inTrack = closestBranch.track;
                }
                else
                {
                    track.outTrackFirst = closestBranch.first;
                    track.outTrack = closestBranch.track;
                }

                if (closestBranch.first)
                {
                    closestBranch.track.inTrackFirst = branch.first;
                    closestBranch.track.inTrack = branch.track;
                }
                else
                {
                    closestBranch.track.outTrackFirst = branch.first;
                    closestBranch.track.outTrack = branch.track;
                }
            }

            MovePointToBranchEnd(bezierPoint, first ? new Branch(track.inTrack, track.inTrackFirst) : new Branch(track.outTrack, track.outTrackFirst));
        }

        private static Branch FindClosestBranch(Object self, Vector3 fromPoint, float maxRange = 5f)
        {
            float num1 = float.PositiveInfinity;
            Track[] tracks = Object.FindObjectsOfType<Track>();
            Track closestTrack = null;
            bool first = false;
            foreach (Track track in tracks)
            {
                if (track == self || !track.Curve || track.Curve.pointCount < 2) continue;
                BezierPoint bezierPoint1 = track.Curve[0];
                BezierPoint bezierPoint2 = track.Curve.Last();
                float num2 = Vector3.SqrMagnitude(fromPoint - bezierPoint1.position);
                if (num2 < maxRange * maxRange && num2 < num1)
                {
                    num1 = num2;
                    closestTrack = track;
                    first = true;
                }

                float num3 = Vector3.SqrMagnitude(fromPoint - bezierPoint2.position);
                if (num3 < maxRange * maxRange && num3 < num1)
                {
                    num1 = num3;
                    closestTrack = track;
                    first = false;
                }
            }

            return closestTrack == null ? null : new Branch(closestTrack, first);
        }

        private static Switch FindClosestJunction(Vector3 point)
        {
            float num1 = float.PositiveInfinity;
            Switch[] objectsOfType = Object.FindObjectsOfType<Switch>();
            Switch closestJunction = null;
            foreach (Switch junction in objectsOfType)
                if (!(junction == null))
                {
                    float num2 = Vector3.SqrMagnitude(point - junction.transform.position);
                    if (!(num2 <= MAX_CONNECT_RANGE * MAX_CONNECT_RANGE) || !(num2 < num1)) continue;
                    num1 = num2;
                    closestJunction = junction;
                }

            return closestJunction;
        }

        private static void MovePointToBranchEnd(BezierPoint bezierPoint, Branch branch)
        {
            if (branch != null && branch.track) bezierPoint.position = branch.GetNode().position;
        }

        private static void MoveInToConnected(Track track)
        {
            Transform inNodeT = track.GetInNodeTransform();
            if (!inNodeT) return;
            track.Curve[0].position = inNodeT.position;
        }

        private static void MoveOutToConnected(Track track)
        {
            Transform outNodeT = track.GetOutNodeTransform();
            if (!outNodeT) return;
            track.Curve.Last().position = outNodeT.position;
        }

        private class Branch
        {
            public readonly bool first;
            public readonly Track track;

            public Branch(Track track, bool first)
            {
                this.track = track;
                this.first = first;
            }

            public BezierPoint GetBezierPoint()
            {
                if (!track) return null;
                return first ? track.Curve[0] : track.Curve.Last();
            }

            public Transform GetNode()
            {
                BezierPoint point = GetBezierPoint();
                return !point ? null : point.transform;
            }
        }
    }
}
