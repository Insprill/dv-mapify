using System.Collections.Generic;
using System.Linq;
using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor
{
    public abstract class SwitchBase: MonoBehaviour
    {
        public virtual Track[] GetTracks()
        {
            return gameObject.GetComponentsInChildren<Track>();
        }

        // TODO why is this unused
//         protected void Snap()
//         {
// #if UNITY_EDITOR
//             BezierPoint[] bezierPoints = FindObjectsOfType<BezierPoint>();
//             GameObject[] selectedObjects = Selection.gameObjects;
//             bool isSelected = selectedObjects.Contains(gameObject);
//
//             TrySnap(bezierPoints, isSelected, transform);
//             foreach (var track in Tracks)
//             {
//                 TrySnap(bezierPoints, isSelected, track.Curve.Last().transform);
//             }
// #endif
//         }
//
//         private void TrySnap(IEnumerable<BezierPoint> points, bool move, Transform reference)
//         {
//             var referencePosition = reference.position;
//             var closestPos = Vector3.zero;
//             var closestDist = float.MaxValue;
//
//             foreach (var otherPoint in points)
//             {
//                 if (otherPoint.Curve().GetComponentInParent<SwitchBase>() == this) continue;
//
//                 var otherPosition = otherPoint.transform.position;
//                 var dist = Mathf.Abs(Vector3.Distance(otherPosition, referencePosition));
//
//                 if (dist > Track.SNAP_RANGE || dist >= closestDist) continue;
//
//                 var aTrack = otherPoint.GetComponentInParent<Track>();
//                 if (aTrack.IsSwitch) continue;
//
//                 closestPos = otherPosition;
//                 closestDist = dist;
//             }
//
//             if (closestDist >= float.MaxValue) return;
//
//             if (move) {
//                 transform.position = closestPos + (transform.position - reference.position);
//             }
//         }
    }
}
