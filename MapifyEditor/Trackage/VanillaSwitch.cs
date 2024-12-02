using UnityEngine;

namespace Mapify.Editor
{
    [RequireComponent(typeof(VanillaObject))]
    public class VanillaSwitch : SwitchBase
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

        public Track ThroughTrack => transform.Find("[track through]").GetComponent<Track>();
        public Track DivergingTrack => transform.Find("[track diverging]").GetComponent<Track>();
        public bool IsLeft => DivergingTrack.Curve.Last().localPosition.x < 0;

        public BezierPoint GetJointPoint() => ThroughTrack.Curve[0];
        public BezierPoint GetThroughPoint() => ThroughTrack.Curve[1];
        public BezierPoint GetDivergingPoint() => DivergingTrack.Curve[1];
        public BezierPoint GetDivergeJoinPoint() => DivergingTrack.Curve[0];
    }
}
