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

        [Tooltip("Which way the switch should be flipped by default")]
        public byte defaultBranch = 0;

        [Tooltip("Which side of the switch the stand will appear on")]
        public StandSide standSide;

        [Tooltip("Tracks in the switch, from left to right")]
        public new Track[] Tracks;
    }
}
