using UnityEngine;

namespace Mapify.Editor
{
    public class CustomSwitch: SwitchBase
    {
        public enum StandSide
        {
            LEFT,
            RIGHT
        }

        public BezierPoint JointPoint => Tracks[0].Curve[0];

        [Tooltip("Which way the switch should be flipped by default")]
        public byte defaultBranch = 0;

        [Tooltip("Which side of the switch the stand will appear on")]
        public StandSide standSide;

        [Tooltip("Tracks in the switch, from left to right")]
        [SerializeField]
        private Track[] Tracks;

        public override Track[] GetTracks()
        {
            return Tracks;
        }

        public void SetTracks(Track[] newTracks)
        {
            Tracks = newTracks;
        }
    }
}
