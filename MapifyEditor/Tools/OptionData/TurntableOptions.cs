using Mapify.Editor.Utils;
using System;
using UnityEngine;

namespace Mapify.Editor.Tools.OptionData
{
    [Serializable]
    public class TurntableOptions
    {
        private float _turntableRadius;
        private float _turntableDepth;
        private float _rotationOffset;
        private float _tracksOffset;
        private float _angleBetweenExits;
        private int _exitTrackCount;
        private float _exitTrackLength;

        public float TurntableRadius { get => _turntableRadius; set => _turntableRadius = Mathf.Max(value, 0.0f); }
        public float TurntableDepth { get => _turntableDepth; set => _turntableDepth = value; }
        public float RotationOffset { get => _rotationOffset; set => _rotationOffset = MathHelper.ClampAngle(value); }
        public float TracksOffset { get => _tracksOffset; set => _tracksOffset = MathHelper.ClampAngle(value); }
        public float AngleBetweenExits { get => _angleBetweenExits; set => _angleBetweenExits = MathHelper.ClampAngle(value); }
        public int ExitTrackCount { get => _exitTrackCount; set => _exitTrackCount = Mathf.Max(value, 0); }
        public float ExitTrackLength { get => _exitTrackLength; set => _exitTrackLength = Mathf.Max(value, 0.0f); }

        public static TurntableOptions DefaultOptions { get; private set; } = new TurntableOptions();

        public TurntableOptions(float turntableRadius = 12.324f, float turntableDepth = 2.75f, float rotationOffset = 0.0f,
            float tracksOffset = -30.0f, float angleBetweenExits = 10.0f, int exitTrackCount = 7, float exitTrackLength = 48.0f)
        {
            TurntableRadius = turntableRadius;
            TurntableDepth = turntableDepth;
            RotationOffset = rotationOffset;
            TracksOffset = tracksOffset;
            AngleBetweenExits = angleBetweenExits;
            ExitTrackCount = exitTrackCount;
            ExitTrackLength = exitTrackLength;
        }
    }
}
