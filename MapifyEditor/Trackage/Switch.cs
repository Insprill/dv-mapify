using System.Collections.Generic;
using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(VanillaObject))]
    public class Switch : SwitchBase
    {
        //must match DV.RailTrack.RailTrack.JUNCTION_DIVERGING_TRACK_NAME / JUNCTION_THROUGH_TRACK_NAME
        public const string THROUGH_TRACK_NAME = "[track through]";
        public const string DIVERGING_TRACK_NAME = "[track diverging]";

        public enum StandSide
        {
            THROUGH,
            DIVERGING
        }

        [Tooltip("Which side of the switch the stand will appear on")]
        public StandSide standSide;

        [Tooltip("Which way the switch should be flipped by default")]
        public StandSide defaultState;

        public Track ThroughTrack => transform.Find(THROUGH_TRACK_NAME).GetComponent<Track>();
        public Track DivergingTrack => transform.Find(DIVERGING_TRACK_NAME).GetComponent<Track>();
        public bool IsLeft => DivergingTrack.Curve.Last().localPosition.x < 0;

        public override BezierPoint GetJointPoint() => ThroughTrack.Curve[0];
        public BezierPoint GetThroughPoint() => ThroughTrack.Curve[1];
        public BezierPoint GetDivergingPoint() => DivergingTrack.Curve[1];
        public BezierPoint GetDivergeJoinPoint() => DivergingTrack.Curve[0];

        public override List<BezierPoint> GetPoints()
        {
            return new List<BezierPoint> { GetJointPoint(), GetThroughPoint(), GetDivergingPoint() };
        }
    }
}
