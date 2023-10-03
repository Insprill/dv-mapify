using Mapify.Editor.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mapify.Editor.Tools
{
    public static class TrackToolsEditor
    {
        /// <summary>
        /// Merges any connected <see cref="Track"/>s in the array.
        /// </summary>
        /// <param name="tracks">The tracks to merge.</param>
        /// <param name="distance">The maximum distance to consider 2 tracks connected.</param>
        /// <param name="registerUndo">Whether to register the merges and deletions as part of the <see cref="Undo"/> calls.</param>
        /// <returns>All remaining tracks (merged and not merged).</returns>
        public static Track[] MergeTracks(IEnumerable<Track> tracks, float distance, bool registerUndo)
        {
            // Square.
            distance *= distance;

            // Remove any switches or turntables and all null tracks.
            List<Track> tracksToMerge = tracks.Where(x => x && !x.IsSwitch && !x.IsTurntable).ToList();
            List<Track> tracksToDelete = new List<Track>();
            Track t;
            BezierCurve curve0;
            BezierCurve curve1;
            bool fromStart0;
            bool fromStart1;

            for (int i = 0; i < tracksToMerge.Count; i++)
            {
                for (int j = i + 1; j < tracksToMerge.Count; j++)
                {
                    curve0 = tracksToMerge[i].Curve;
                    curve1 = tracksToMerge[j].Curve;

                    // Check if the start or end points match.
                    if ((curve0[0].position - curve1[0].position).sqrMagnitude < distance)
                    {
                        fromStart0 = true;
                        fromStart1 = false;

                    }
                    else if ((curve0[0].position - curve1.Last().position).sqrMagnitude < distance)
                    {
                        fromStart0 = true;
                        fromStart1 = true;
                    }
                    else if ((curve0.Last().position - curve1[0].position).sqrMagnitude < distance)
                    {
                        fromStart0 = false;
                        fromStart1 = true;
                    }
                    else if ((curve0.Last().position - curve1.Last().position).sqrMagnitude < distance)
                    {
                        fromStart0 = false;
                        fromStart1 = false;
                    }
                    else
                    {
                        // None match, do not connect.
                        continue;
                    }

                    // Mark the soon-to-be-merged tracks for deletion and remove them from possible merges.
                    tracksToDelete.Add(tracksToMerge[j]);
                    tracksToDelete.Add(tracksToMerge[i]);
                    tracksToMerge.RemoveAt(j);
                    tracksToMerge.RemoveAt(i);

                    // Create a new track as the merged one.
                    t = new GameObject("Merged Track").AddComponent<Track>();
                    t.transform.position = fromStart0 ?
                        (fromStart1 ? curve1.Last().position : curve1[0].position) :
                        curve0[0].position;

                    Merge(curve0, fromStart0, curve1, fromStart1, t.Curve);

                    // If both have the same parent, keep the hierarchy for the new track.
                    if (curve0.transform.parent == curve1.transform.parent)
                    {
                        t.Curve.transform.parent = curve0.transform.parent;
                    }

#if UNITY_EDITOR
                    if (registerUndo)
                    {
                        Undo.RegisterCreatedObjectUndo(t.gameObject, "Create Merged Track");
                    }
#endif

                    tracksToMerge.Add(t);

                    // After a successful merge, restart the whole thing.
                    i = -1;
                    break;
                }
            }

            for (int i = 0; i < tracksToDelete.Count; i++)
            {

#if UNITY_EDITOR
                if (registerUndo)
                {
                    Undo.DestroyObjectImmediate(tracksToDelete[i].gameObject);
                    continue;
                }
#endif
                Object.DestroyImmediate(tracksToDelete[i].gameObject);
            }

            return tracksToMerge.ToArray();
        }

        private static void Merge(BezierCurve curve0, bool fromStart0, BezierCurve curve1, bool fromStart1, BezierCurve newCurve)
        {
            BezierPoint bpO;
            BezierPoint bpN;

            // Add the points of the 2 curves to the new one.
            // Order changes based on which points are connected.
            if (fromStart0)
            {
                for (int i = 0; i < curve1.pointCount; i++)
                {
                    bpO = curve1[fromStart1 ? i : curve1.pointCount - 1 - i];
                    bpN = newCurve.AddPointAt(bpO.position);
                    SetPointToOtherPoint(bpO, bpN, !fromStart1);
                }

                bpN = newCurve.Last();
                bpN.handleStyle = BezierPoint.HandleStyle.Broken;
                bpN.globalHandle2 = curve0[0].globalHandle2;

                for (int j = 1; j < curve0.pointCount; j++)
                {
                    bpO = curve0[j];
                    bpN = newCurve.AddPointAt(bpO.position);
                    SetPointToOtherPoint(bpO, bpN, false);
                }
            }
            else
            {
                for (int i = 0; i < curve0.pointCount; i++)
                {
                    bpO = curve0[i];
                    bpN = newCurve.AddPointAt(bpO.position);
                    SetPointToOtherPoint(bpO, bpN, false);
                }

                bpN = newCurve.Last();
                bpN.handleStyle = BezierPoint.HandleStyle.Broken;
                bpN.globalHandle2 = fromStart1 ? curve1[0].globalHandle2 : curve1.Last().globalHandle1;

                for (int j = 1; j < curve1.pointCount; j++)
                {
                    bpO = curve1[fromStart1 ? j : curve1.pointCount - 1 - j];
                    bpN = newCurve.AddPointAt(bpO.position);
                    SetPointToOtherPoint(bpO, bpN, !fromStart1);
                }
            }
        }

        private static void SetPointToOtherPoint(BezierPoint from, BezierPoint to, bool reverse)
        {
            to.position = from.position;
            to.handleStyle = from.handleStyle;

            if (reverse)
            {
                to.globalHandle1 = from.globalHandle2;
                to.globalHandle2 = from.globalHandle1;
            }
            else
            {
                to.globalHandle1 = from.globalHandle1;
                to.globalHandle2 = from.globalHandle2;
            }
        }

        public static void MatchTrackToTerrain(Track t, float heightOffset, float maxDistance, float reverseOffset, float normalMatch)
        {
            if (t.IsSwitch || t.IsTurntable)
            {
                return;
            }

            Vector3 newOffset = Vector3.up * heightOffset;
            Vector3 hitOffset = Vector3.up * reverseOffset;
            RaycastHit hit;
            BezierPoint bp;

            for (int i = 0; i < t.Curve.pointCount; i++)
            {
                bp = t.Curve[i];

                if (Physics.Raycast(bp.position + hitOffset, Vector3.down, out hit, maxDistance + reverseOffset))
                {
                    bp.position = hit.point + newOffset;

                    bp.globalHandle1 = Vector3.Lerp(bp.globalHandle1,
                        TrackToolsHelper.HandleMatchNormal(bp.position, bp.globalHandle1, hit.normal), normalMatch);
                    bp.globalHandle2 = Vector3.Lerp(bp.globalHandle2,
                        TrackToolsHelper.HandleMatchNormal(bp.position, bp.globalHandle2, hit.normal), normalMatch);
                }
            }
        }

        public static void CreatePointBetween2(Track t, int point, float f)
        {
            Vector3[][] curves = MathHelper.SplitBezier(t.Curve.AsControlPoints(point), f);

            BezierPoint bp0 = t.Curve[point];
            BezierPoint bp2 = t.Curve[point + 1];
            BezierPoint bp1 = t.Curve.InsertPointAt(point + 1, curves[0][3]);

            bp0.handleStyle = BezierPoint.HandleStyle.Broken;
            bp1.handleStyle = BezierPoint.HandleStyle.Broken;
            bp2.handleStyle = BezierPoint.HandleStyle.Broken;

            bp0.globalHandle2 = curves[0][1];
            bp1.globalHandle1 = curves[0][2];
            bp1.globalHandle2 = curves[1][1];
            bp2.globalHandle1 = curves[1][2];
        }
    }
}
