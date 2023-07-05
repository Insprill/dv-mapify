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

        public Track ThroughTrack => transform.Find("[track through]").GetComponent<Track>();
        public Track DivergingTrack => transform.Find("[track diverging]").GetComponent<Track>();
        public bool IsLeft => DivergingTrack.Curve.Last().localPosition.x < 0;

        private void OnDrawGizmos()
        {
            if (transform.DistToSceneCamera() >= Track.SNAP_UPDATE_RANGE_SQR)
                return;
            Snap();
        }

        public void Snap()
        {
#if UNITY_EDITOR
            BezierPoint[] bezierPoints = FindObjectsOfType<BezierPoint>();
            GameObject[] selectedObjects = Selection.gameObjects;
            bool isSelected = selectedObjects.Contains(gameObject);
            TrySnap(bezierPoints, isSelected, 0);
            TrySnap(bezierPoints, isSelected, 1);
            TrySnap(bezierPoints, isSelected, 2);
#endif
        }

        private void TrySnap(IEnumerable<BezierPoint> points, bool move, byte which)
        {
            Transform reference;
            switch (which)
            {
                case 0:
                    reference = transform;
                    break;
                case 1:
                    reference = DivergingTrack.Curve.Last().transform;
                    break;
                case 2:
                    reference = ThroughTrack.Curve.Last().transform;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(which));
            }

            Vector3 pos = reference.position;
            Vector3 closestPos = Vector3.zero;
            float closestDist = float.MaxValue;
            foreach (BezierPoint otherBP in points)
            {
                if (otherBP.Curve().GetComponentInParent<Switch>() == this) continue;
                Vector3 otherPos = otherBP.transform.position;
                float dist = Mathf.Abs(Vector3.Distance(otherPos, pos));
                if (dist > Track.SNAP_RANGE || dist >= closestDist) continue;
                if (otherBP.GetComponentInParent<Track>().IsSwitch) continue;
                closestPos = otherPos;
                closestDist = dist;
            }

            if (closestDist >= float.MaxValue) return;

            if (move)
                transform.position = closestPos + (transform.position - reference.position);
        }
    }
}
