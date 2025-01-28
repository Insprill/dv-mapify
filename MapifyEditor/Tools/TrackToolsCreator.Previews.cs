using Mapify.Editor.Tools.OptionData;
using Mapify.Editor.Utils;
using System.Collections.Generic;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

namespace Mapify.Editor.Tools
{
    public static partial class TrackToolsCreator
    {
        /// <summary>
        /// Class with methods to create preview lines for track pieces.
        /// </summary>
        internal static class Previews
        {
            // Not too elegant, but it's the easiest way to calculate the lengths of everything without having
            // to repeat the process.
            internal struct YardCache
            {
                public float TotalLength;
                public float[] SidingsLength;
                public float LoadingGauge;

                public YardCache(float totalLength, float[] sidingsLength, float loadingGauge)
                {
                    TotalLength = totalLength;
                    SidingsLength = sidingsLength;
                    LoadingGauge = loadingGauge;
                }
            }

            internal static YardCache? CachedYard = null;

            public static Vector3[] PreviewStraight(Vector3 attachPoint, Vector3 handlePosition, float length, float endGrade,
                out Vector3[] points, int samples = 8)
            {
                var curves = GenerateStraightBezier(attachPoint, handlePosition, length, endGrade);
                List<Vector3> lines = new List<Vector3>();
                List<Vector3> ps = new List<Vector3>();

                for (int i = 0; i < curves.Length; i += 4)
                {
                    ps.Add(curves[i].P0);
                    lines.AddRange(curves[i].Sample(samples));
                }

                points = ps.ToArray();
                return lines.ToArray();
            }

            public static Vector3[] PreviewArcCurve(Vector3 attachPoint, Vector3 handlePosition, TrackOrientation orientation, float radius,
                float arc, float maxArc, float endGrade, out Vector3[] points, int samples = 8)
            {
                var curves = GenerateCurveBeziers(attachPoint, handlePosition, orientation, radius, arc, maxArc, endGrade);
                List<Vector3> lines = new List<Vector3>();
                List<Vector3> ps = new List<Vector3> { curves[0].P0 };

                for (int i = 0; i < curves.Length; i++)
                {
                    ps.Add(curves[i].P0);
                    lines.AddRange(curves[i].Sample(samples));
                }

                points = ps.ToArray();
                return lines.ToArray();
            }

            public static Vector3[][] PreviewSwitch(Switch prefab, Vector3 attachPoint, Vector3 handlePosition, SwitchPoint connectingPoint,
                int samples = 8)
            {
                var curves = TrackToolsHelper.GetSwitchBeziers(prefab, attachPoint, handlePosition, connectingPoint);

                return new[]
                {
                    MathHelper.SampleBezier(curves[0], samples),
                    MathHelper.SampleBezier(curves[1], samples)
                };
            }

            public static Vector3[][] PreviewCustomSwitch(Vector3 attachPoint, Vector3 handlePosition, int switchBranchesCount, int connectingPoint, float radius, float arc, float endGrade,
                int samples = 8)
            {
                //TODO implement connectingPoint

                var curves = new Vector3[switchBranchesCount][];
                var length = radius * arc * Mathf.Deg2Rad;

                for (int branchIndex = 0; branchIndex < switchBranchesCount; branchIndex++)
                {
                    if (switchBranchesCount % 2 == 1 && branchIndex == (switchBranchesCount-1) / 2)
                    {
                        //middle track
                        curves[branchIndex] = PreviewStraight(attachPoint, handlePosition, length, endGrade, out _);
                        continue;
                    }

                    TrackOrientation trackOrientation;
                    float thisRadius;

                    if (branchIndex < switchBranchesCount / 2.0)
                    {
                        //left of center
                        trackOrientation = TrackOrientation.Left;
                        thisRadius = (branchIndex + 1) * radius;
                    }
                    else
                    {
                        //right of center
                        trackOrientation = TrackOrientation.Right;
                        thisRadius = (switchBranchesCount - branchIndex) * radius;
                    }

                    var thisArc = length / thisRadius * Mathf.Rad2Deg;
                    curves[branchIndex] = PreviewArcCurve(attachPoint, handlePosition, trackOrientation, thisRadius, thisArc, 360, endGrade, out _, samples);
                }

                return curves;
            }

            public static Vector3[][] PreviewYard(Switch leftPrefab, Switch rightPrefab, Vector3 attachPoint, Vector3 handlePosition,
                TrackOrientation orientation, float trackDistance, YardOptions yardOptions, int samples = 8)
            {
                if (yardOptions.Half)
                {
                    return PreviewHalfYard(leftPrefab, rightPrefab, attachPoint, handlePosition, orientation, trackDistance, yardOptions, samples);
                }
                else
                {
                    return PreviewFullYard(leftPrefab, rightPrefab, attachPoint, handlePosition, orientation, trackDistance, yardOptions, samples);
                }
            }

            private static Vector3[][] PreviewFullYard(Switch leftPrefab, Switch rightPrefab, Vector3 attachPoint, Vector3 handlePosition,
                TrackOrientation orientation, float trackDistance, YardOptions yardOptions, int samples)
            {
                List<Vector3[]> results = new List<Vector3[]>();
                Vector3[][] temp;
                Vector3 offset;
                Vector3 dir = attachPoint - handlePosition;
                Vector3 mid0;
                Vector3 mid1;

                Vector3[] points1 = System.Array.Empty<Vector3>();
                Vector3[] points2 = System.Array.Empty<Vector3>();
                Vector3[] points3 = System.Array.Empty<Vector3>();
                Vector3[] points4 = System.Array.Empty<Vector3>();

                TrackOrientation currentOrientation = orientation;
                temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance,
                    true, false, out points1, samples);
                results.AddRange(temp);
                mid0 = temp[0][1];

                if (yardOptions.TracksOtherSide > 0)
                {
                    currentOrientation = FlipOrientation(orientation);
                    offset = new Vector3(0, 0, (currentOrientation == TrackOrientation.Left ? leftPrefab : rightPrefab).ThroughTrack.Curve[1].position.z +
                        TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance));

                    results.Add(new Vector3[] { temp[0][1], temp[0][1] + offset });

                    temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance, false, false, out points2, samples);

                    for (int i = 0; i < temp.Length; i++)
                    {
                        for (int j = 0; j < temp[i].Length; j++)
                        {
                            temp[i][j] += offset;
                        }
                    }

                    for (int i = 0; i < points2.Length; i++)
                    {
                        points2[i] += offset;
                    }

                    results.AddRange(temp);
                    mid0 = temp[0][1];
                }

                int half = results.Count;

                if (yardOptions.AlternateSides && yardOptions.TracksOtherSide > 0)
                {
                    currentOrientation = FlipOrientation(orientation);

                    temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance, false, true, out points4, samples);
                    results.AddRange(temp);

                    currentOrientation = orientation;
                    offset = new Vector3(0, 0, -(currentOrientation == TrackOrientation.Left ? leftPrefab : rightPrefab).ThroughTrack.Curve[1].position.z -
                        TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance));

                    results.Add(new Vector3[] { temp[0][1], temp[0][1] + offset });

                    temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance, true, true, out points3, samples);

                    for (int i = 0; i < temp.Length; i++)
                    {
                        for (int j = 0; j < temp[i].Length; j++)
                        {
                            temp[i][j] += offset;
                        }
                    }

                    for (int i = 0; i < points3.Length; i++)
                    {
                        points3[i] += offset;
                    }

                    results.AddRange(temp);
                    mid1 = temp[0][1];
                }
                else
                {
                    currentOrientation = orientation;

                    temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance, true, true, out points3, samples);
                    results.AddRange(temp);
                    mid1 = temp[0][1];

                    if (yardOptions.TracksOtherSide > 0)
                    {
                        currentOrientation = FlipOrientation(orientation);
                        offset = new Vector3(0, 0, -(currentOrientation == TrackOrientation.Left ? leftPrefab : rightPrefab).ThroughTrack.Curve[1].position.z -
                            TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance));

                        results.Add(new Vector3[] { temp[0][1], temp[0][1] + offset });

                        temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance, false, true, out points4, samples);

                        for (int i = 0; i < temp.Length; i++)
                        {
                            for (int j = 0; j < temp[i].Length; j++)
                            {
                                temp[i][j] += offset;
                            }
                        }

                        for (int i = 0; i < points4.Length; i++)
                        {
                            points4[i] += offset;
                        }

                        results.AddRange(temp);
                        mid1 = temp[0][1];
                    }
                }

                float move = 0;
                float dist = mid1.z - mid0.z;

                if (dist < yardOptions.MinimumLength)
                {
                    move = Mathf.Max(move, -dist + yardOptions.MinimumLength);
                }

                for (int i = 0; i < points1.Length; i++)
                {
                    dist = points3[i].z - points1[i].z;

                    if (dist < yardOptions.MinimumLength)
                    {
                        move = Mathf.Max(move, -dist + yardOptions.MinimumLength);
                    }
                }

                for (int i = 0; i < points2.Length; i++)
                {
                    dist = points4[i].z - points2[i].z;

                    if (dist < yardOptions.MinimumLength)
                    {
                        move = Mathf.Max(move, -dist + yardOptions.MinimumLength);
                    }
                }

                offset = new Vector3(0, 0, move);

                for (int i = half; i < results.Count; i++)
                {
                    for (int j = 0; j < results[i].Length; j++)
                    {
                        results[i][j] += offset;
                    }
                }

                // TODO: fix lengths.
                // Calculate lengths.
                Vector3[] magnitudeHelper;
                List<float> sidingLengths = new List<float>();
                // Account for merged tracks.
                float extraLength = -TrackToolsHelper.CalculateLengthFromDistanceYardSides(leftPrefab, trackDistance);
                extraLength += leftPrefab.DivergingTrack.Curve.length;
                extraLength *= 2.0f;

                for (int i = 0; i < points1.Length; i++)
                {
                    magnitudeHelper = new Vector3[] { points1[i], points3[i] + offset };
                    results.Add(magnitudeHelper);
                    sidingLengths.Add((magnitudeHelper[1] - magnitudeHelper[0]).magnitude);
                }

                sidingLengths[sidingLengths.Count - 1] += extraLength;

                magnitudeHelper = new Vector3[] { mid0, mid1 + offset };
                results.Add(magnitudeHelper);
                sidingLengths.Add((magnitudeHelper[1] - magnitudeHelper[0]).magnitude - 48.0f);

                for (int i = 0; i < points2.Length; i++)
                {
                    magnitudeHelper = new Vector3[] { points2[i], points4[i] + offset };
                    results.Add(magnitudeHelper);
                    sidingLengths.Add((magnitudeHelper[1] - magnitudeHelper[0]).magnitude);
                }

                Quaternion rot = Quaternion.LookRotation(dir);

                for (int i = 0; i < results.Count; i++)
                {
                    for (int j = 0; j < results[i].Length; j++)
                    {
                        results[i][j] = (rot * results[i][j]) + attachPoint;
                    }
                }

                sidingLengths[sidingLengths.Count - 1] += extraLength;

                MapInfo mapInfo = null;
#if UNITY_EDITOR
                mapInfo = EditorAssets.FindAsset<MapInfo>();
#endif
                CachedYard = new YardCache((results[half][0] - results[0][0]).magnitude, sidingLengths.ToArray(),
                    mapInfo ? mapInfo.loadingGaugeWidth : 3.0f);

                return results.ToArray();
            }

            private static Vector3[][] PreviewHalfYard(Switch leftPrefab, Switch rightPrefab, Vector3 attachPoint, Vector3 handlePosition,
                TrackOrientation orientation, float trackDistance, YardOptions yardOptions, int samples)
            {
                List<Vector3[]> results = new List<Vector3[]>();
                Vector3[][] temp;
                Vector3 offset;
                Vector3 dir = attachPoint - handlePosition;
                Vector3 mid0;
                Vector3 mid1;

                Vector3[] points1 = System.Array.Empty<Vector3>();
                Vector3[] points2 = System.Array.Empty<Vector3>();
                Vector3[] points3 = System.Array.Empty<Vector3>();
                Vector3[] points4 = System.Array.Empty<Vector3>();

                TrackOrientation currentOrientation = orientation;
                temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance,
                    true, false, out points1, samples);
                results.AddRange(temp);
                mid0 = temp[0][1];

                if (yardOptions.TracksOtherSide > 0)
                {
                    currentOrientation = FlipOrientation(orientation);
                    offset = new Vector3(0, 0, (currentOrientation == TrackOrientation.Left ? leftPrefab : rightPrefab).ThroughTrack.Curve[1].position.z +
                        TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance));

                    results.Add(new Vector3[] { temp[0][1], temp[0][1] + offset });

                    temp = PreviewSwitchSprawl(leftPrefab, rightPrefab, yardOptions, currentOrientation, trackDistance, false, false, out points2, samples);

                    for (int i = 0; i < temp.Length; i++)
                    {
                        for (int j = 0; j < temp[i].Length; j++)
                        {
                            temp[i][j] += offset;
                        }
                    }

                    for (int i = 0; i < points2.Length; i++)
                    {
                        points2[i] += offset;
                    }

                    results.AddRange(temp);
                    mid0 = temp[0][1];
                }

                int half = results.Count;

                points3 = new Vector3[points1.Length];

                for (int i = 0; i < points1.Length; i++)
                {
                    points3[i] = new Vector3(points1[i].x, points1[i].y, 0);
                }

                mid1 = Vector3.zero;

                points4 = new Vector3[points2.Length];

                for (int i = 0; i < points2.Length; i++)
                {
                    points4[i] = new Vector3(points2[i].x, points2[i].y, 0);
                }

                float move = 0;
                float dist = mid1.z - mid0.z;

                if (dist < yardOptions.MinimumLength)
                {
                    move = Mathf.Max(move, -dist + yardOptions.MinimumLength);
                }

                for (int i = 0; i < points1.Length; i++)
                {
                    dist = points3[i].z - points1[i].z;

                    if (dist < yardOptions.MinimumLength)
                    {
                        move = Mathf.Max(move, -dist + yardOptions.MinimumLength);
                    }
                }

                for (int i = 0; i < points2.Length; i++)
                {
                    dist = points4[i].z - points2[i].z;

                    if (dist < yardOptions.MinimumLength)
                    {
                        move = Mathf.Max(move, -dist + yardOptions.MinimumLength);
                    }
                }

                offset = new Vector3(0, 0, move);

                for (int i = half; i < results.Count; i++)
                {
                    for (int j = 0; j < results[i].Length; j++)
                    {
                        results[i][j] += offset;
                    }
                }

                // TODO: fix lengths.
                // Calculate lengths.
                Vector3[] magnitudeHelper;
                List<float> sidingLengths = new List<float>();
                // Account for merged tracks.
                float extraLength = -TrackToolsHelper.CalculateLengthFromDistanceYardSides(leftPrefab, trackDistance);
                extraLength += leftPrefab.DivergingTrack.Curve.length;
                extraLength *= 2.0f;

                for (int i = 0; i < points1.Length; i++)
                {
                    magnitudeHelper = new Vector3[] { points1[i], points3[i] + offset };
                    results.Add(magnitudeHelper);
                    sidingLengths.Add((magnitudeHelper[1] - magnitudeHelper[0]).magnitude);
                }

                sidingLengths[sidingLengths.Count - 1] += extraLength;

                magnitudeHelper = new Vector3[] { mid0, mid1 + offset };
                results.Add(magnitudeHelper);
                sidingLengths.Add((magnitudeHelper[1] - magnitudeHelper[0]).magnitude - 48.0f);

                for (int i = 0; i < points2.Length; i++)
                {
                    magnitudeHelper = new Vector3[] { points2[i], points4[i] + offset };
                    results.Add(magnitudeHelper);
                    sidingLengths.Add((magnitudeHelper[1] - magnitudeHelper[0]).magnitude);
                }

                Quaternion rot = Quaternion.LookRotation(dir);

                for (int i = 0; i < results.Count; i++)
                {
                    for (int j = 0; j < results[i].Length; j++)
                    {
                        results[i][j] = (rot * results[i][j]) + attachPoint;
                    }
                }

                sidingLengths[sidingLengths.Count - 1] += extraLength;

                MapInfo mapInfo = null;
#if UNITY_EDITOR
                mapInfo = EditorAssets.FindAsset<MapInfo>();
#endif
                CachedYard = new YardCache((results[half][0] - results[0][0]).magnitude, sidingLengths.ToArray(),
                    mapInfo ? mapInfo.loadingGaugeWidth : 3.0f);

                return results.ToArray();
            }

            private static Vector3[][] PreviewSwitchSprawl(Switch leftPrefab, Switch rightPrefab, YardOptions yardOptions, TrackOrientation orientation,
                float trackDistance, bool mainSide, bool reverse, out Vector3[] trackPoints, int samples = 8)
            {
                List<Vector3[]> results = new List<Vector3[]>();
                List<Vector3> points = new List<Vector3>();

                Switch current = orientation == TrackOrientation.Left ? leftPrefab : rightPrefab;

                Vector3[][] temp = PreviewSwitch(current, Vector3.zero, Vector3.back, SwitchPoint.Joint, samples);
                results.AddRange(temp);

                BezierPoint bp = current.GetDivergingPoint();
                Vector3 div = temp[1][temp[1].Length - 1];
                Vector3 dir = bp.globalHandle1 - bp.position;
                dir = dir.normalized;

                int count = mainSide ? yardOptions.TracksMainSide : yardOptions.TracksOtherSide;

                current = orientation == TrackOrientation.Left ? rightPrefab : leftPrefab;

                float lengthS = -TrackToolsHelper.CalculateLengthFromDistanceYardSides(leftPrefab, trackDistance);

                Vector3[] straight = StraightNormal(div, dir, -TrackToolsHelper.CalculateLengthFromDistanceYardCentre(leftPrefab, trackDistance));

                for (int i = 1; i < count; i++)
                {
                    results.Add(straight);
                    temp = PreviewSwitch(current, straight[1], div, SwitchPoint.Joint, samples);
                    results.AddRange(temp);
                    points.Add(temp[1][temp[1].Length - 1]);
                    div = temp[0][temp[0].Length - 1];
                    straight = StraightNormal(div, dir, lengthS);
                }

                temp = PreviewSwitch(current, straight[1], div, SwitchPoint.Joint, samples);
                results.Add(straight);
                results.Add(temp[1]);
                points.Add(temp[1][temp[1].Length - 1]);

                trackPoints = points.ToArray();

                Vector3[][] final = results.ToArray();

                if (!reverse)
                {
                    return final;
                }

                for (int i = 0; i < final.Length; i++)
                {
                    for (int j = 0; j < final[i].Length; j++)
                    {
                        final[i][j].z = -final[i][j].z;
                    }
                }

                for (int i = 0; i < trackPoints.Length; i++)
                {
                    trackPoints[i].z = -trackPoints[i].z;
                }

                return final;
            }

            private static Vector3[] StraightNormal(Vector3 start, Vector3 dir, float length)
            {
                return new Vector3[] { start, start + dir * length };
            }

            public static Vector3[][] PreviewTurntable(Vector3 attachPoint, Vector3 handlePosition, TurntableOptions turntableOptions, int samples = 8)
            {
                Vector3[][] results = new Vector3[2 + turntableOptions.ExitTrackCount][];

                Vector3 dir = (attachPoint - handlePosition).normalized;
                Vector3 pivot = attachPoint + dir * turntableOptions.TurntableRadius;

                Quaternion rotRoot = Quaternion.AngleAxis(turntableOptions.RotationOffset, Vector3.up);
                dir = rotRoot * dir;

                results[0] = new Vector3[] { pivot + dir * turntableOptions.TurntableRadius, pivot - dir * turntableOptions.TurntableRadius };
                results[1] = MathHelper.SampleCircle(pivot, turntableOptions.TurntableRadius, samples * 2);

                dir = Quaternion.AngleAxis(turntableOptions.TracksOffset, Vector3.up) * dir;
                Quaternion rot = Quaternion.AngleAxis(turntableOptions.AngleBetweenExits, Vector3.up);

                for (int i = 0; i < turntableOptions.ExitTrackCount; i++)
                {
                    results[2 + i] = StraightNormal(pivot + dir * turntableOptions.TurntableRadius, dir, turntableOptions.ExitTrackLength);
                    dir = rot * dir;
                }

                return results;
            }

            public static Vector3[] PreviewConnect2(Vector3 p0, Vector3 h0, Vector3 p1, Vector3 h1, float lengthMultiplier, int samples = 8)
            {
                float length = (p1 - p0).magnitude * MathHelper.OneThird * lengthMultiplier;

                return MathHelper.SampleBezier(
                    p0,
                    p0 - (h0 - p0).normalized * length,
                    p1 - (h1 - p1).normalized * length,
                    p1,
                    samples);
            }

            public static Vector3[][] PreviewCrossover(Switch prefab, Vector3 attachPoint, Vector3 handlePosition,
                TrackOrientation orientation, float trackDistance, bool isTrailing, float switchDistance, int samples = 8)
            {
                List<Vector3[]> results = new List<Vector3[]>();

                SimpleBezier[] temp = TrackToolsHelper.GetSwitchBeziers(prefab, attachPoint, handlePosition,
                    isTrailing ? SwitchPoint.Through : SwitchPoint.Joint);
                results.AddRange(MathHelper.SampleBeziers(temp, samples));
                (Vector3 Point, Vector3 Handle) mid0 = (temp[1].P3, temp[1].P2);

                Vector3 point = temp[0].P3;
                Vector3 dir = (temp[0].P1 - temp[0].P0).normalized;

                Vector3 offset = (orientation == TrackOrientation.Left ?
                    MathHelper.RotateCCW(dir.Flatten()) :
                    MathHelper.RotateCW(dir.Flatten())).To3D(0) * trackDistance;

                Vector3 point2 = point + (dir * switchDistance) + offset;

                temp = TrackToolsHelper.GetSwitchBeziers(prefab, point2, point2 - dir, SwitchPoint.Through);
                results.AddRange(MathHelper.SampleBeziers(temp, samples));
                (Vector3 Point, Vector3 Handle) mid1 = (temp[1].P3, temp[1].P2);

                results.Add(PreviewConnect2(mid0.Point, mid0.Handle, mid1.Point, mid1.Handle, 1.0f, samples));

                return results.ToArray();
            }

            public static Vector3[][] PreviewScissorsCrossover(Switch leftPrefab, Switch rightPrefab, Vector3 attachPoint, Vector3 handlePosition,
                TrackOrientation orientation, float trackDistance, float switchDistance, int samples = 8)
            {
                List<Vector3[]> results = new List<Vector3[]>();

                results.AddRange(PreviewCrossover(orientation == TrackOrientation.Left ? leftPrefab : rightPrefab,
                    attachPoint, handlePosition, orientation, trackDistance, false, switchDistance, samples));

                Vector3 dir = (attachPoint - handlePosition).normalized;
                Vector3 offset = (orientation == TrackOrientation.Left ?
                    MathHelper.RotateCCW(dir.Flatten()) :
                    MathHelper.RotateCW(dir.Flatten())).To3D(0) * trackDistance;

                orientation = FlipOrientation(orientation);

                results.AddRange(PreviewCrossover(orientation == TrackOrientation.Left ? leftPrefab : rightPrefab,
                    attachPoint + offset, handlePosition + offset, orientation, trackDistance, false, switchDistance, samples));

                int last = results[0].Length - 1;

                results.Add(new Vector3[] { results[0][last], results[7][last] });
                results.Add(new Vector3[] { results[5][last], results[2][last] });

                return results.ToArray();
            }

            public static Vector3[][] PreviewDoubleSlip(Switch leftPrefab, Switch rightPrefab, Vector3 attachPoint, Vector3 handlePosition,
                TrackOrientation orientation, float crossAngle, int samples = 8)
            {
                List<Vector3[]> results = new List<Vector3[]>();
                float radius = TrackToolsHelper.CalculateSwitchRadius(leftPrefab);
                float minAngle = TrackToolsHelper.CalculateSwitchAngle(leftPrefab) * Mathf.Rad2Deg;
                float arc = Mathf.Clamp(crossAngle - (minAngle * 2.0f), 0.1f, 90.0f - (minAngle * 2.0f));

                SimpleBezier[] temp = TrackToolsHelper.GetSwitchBeziers(orientation == TrackOrientation.Left ? leftPrefab : rightPrefab,
                    attachPoint, handlePosition, SwitchPoint.Joint);
                results.AddRange(MathHelper.SampleBeziers(temp));
                Vector3 mid00 = temp[0].P3;

                Vector3 dir = temp[0].P3 - temp[0].P0;

                temp = GenerateCurveBeziers(temp[1].P3, temp[1].P2, orientation, radius, arc, 90, 0);
                results.Add(temp[0].Sample(samples));

                temp = TrackToolsHelper.GetSwitchBeziers(orientation == TrackOrientation.Left ? rightPrefab : leftPrefab,
                    temp[0].P3, temp[0].P2, SwitchPoint.Diverging);
                results.AddRange(MathHelper.SampleBeziers(temp));
                Vector3 mid10 = temp[0].P3;

                Vector3 mid = MathHelper.LineLineIntersection(
                    attachPoint.Flatten(), (attachPoint + dir).Flatten(),
                    temp[0].P0.Flatten(), temp[0].P3.Flatten()).To3D(attachPoint.y);
                Vector3 next = MathHelper.MirrorAround(attachPoint, mid);

                temp = TrackToolsHelper.GetSwitchBeziers(orientation == TrackOrientation.Left ? leftPrefab : rightPrefab,
                    next, next + dir, SwitchPoint.Joint);
                results.AddRange(MathHelper.SampleBeziers(temp, samples));
                Vector3 mid01 = temp[0].P3;

                temp = GenerateCurveBeziers(temp[1].P3, temp[1].P2, orientation, radius, arc, 90, 0);
                results.Add(MathHelper.SampleBezier(temp[0], samples));

                temp = TrackToolsHelper.GetSwitchBeziers(orientation == TrackOrientation.Left ? rightPrefab : leftPrefab,
                    temp[0].P3, temp[0].P2, SwitchPoint.Diverging);
                results.AddRange(MathHelper.SampleBeziers(temp));
                Vector3 mid11 = temp[0].P3;

                results.Add(new Vector3[] { mid00, mid01 });
                results.Add(new Vector3[] { mid10, mid11 });

                return results.ToArray();
            }
        }
    }
}
