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
    }
}
