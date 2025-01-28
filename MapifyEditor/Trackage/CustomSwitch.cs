using System;
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

        public override BezierPoint GetJointPoint() => Tracks[0].Curve[0];

        public BezierPoint GetOutPoint(int branchIndex)
        {
            if (branchIndex >= Tracks.Length)
            {
                throw new IndexOutOfRangeException($"Branch index {branchIndex} is out of range. Switch has {Tracks.Length} tracks.");
            }
            return Tracks[branchIndex].Curve.Last();
        }

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
