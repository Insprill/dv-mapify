using System;
using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

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

        public Transform ThroughTrack => transform.Find("[track through]");
        public Transform DivergingTrack => transform.Find("[track diverging]");

        private void OnDrawGizmos()
        {
            if ((transform.position - Camera.current.transform.position).sqrMagnitude >= Track.SNAP_UPDATE_RANGE * Track.SNAP_UPDATE_RANGE)
                return;
            BezierPoint[] bezierPoints = FindObjectsOfType<BezierPoint>();
            GameObject[] selectedObjects = Selection.gameObjects;
            bool isSelected = selectedObjects.Contains(gameObject);
            TrySnap(bezierPoints, isSelected, 0);
            TrySnap(bezierPoints, isSelected, 1);
            TrySnap(bezierPoints, isSelected, 2);
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
                    reference = DivergingTrack.GetComponent<BezierCurve>().Last().transform;
                    break;
                case 2:
                    reference = ThroughTrack.GetComponent<BezierCurve>().Last().transform;
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
                closestPos = otherPos;
                closestDist = dist;
            }

            if (closestDist >= float.MaxValue) return;

            if (move)
                transform.position = closestPos + (transform.position - reference.position);
        }
    }
}
