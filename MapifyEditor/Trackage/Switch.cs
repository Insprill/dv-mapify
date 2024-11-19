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
    [RequireComponent(typeof(VanillaObject))]
    public class Switch : SwitchBase
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
    }
}
