using Mapify.Editor.Utils;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

namespace Mapify.Editor.Tools
{
    public static class TrackToolsHelper
    {
        // 24.0f / Mathf.Sin(Mathf.Atan(1.437454f / 10.80206f))
        // Desmos version:
        // \frac{24}{\sin\left(\arctan\left(\frac{1.437454}{10.80206}\right)\right)}
        /// <summary>
        /// The radius used when curves need to match switches.
        /// </summary>
        public const float DefaultSwitchRadius = 181.9430668f;
        public static Track[] CachedTracks = new Track[0];

        public static bool HasCachedTracks => CachedTracks.Length > 0;

        public static void CreateCache()
        {
            CachedTracks = GetAllActiveTracks();
        }

        /// <summary>
        /// Calculates the height difference between 2 grades over a certain length.
        /// </summary>
        /// <param name="startGrade">The grade at the start.</param>
        /// <param name="endGrade">The grade at the end.</param>
        /// <param name="horizontalLength">The horizontal length.</param>
        /// <returns></returns>
        public static float CalculateHeightDifference(float startGrade, float endGrade, float horizontalLength)
        {
            return (startGrade + endGrade) * 0.5f * horizontalLength;
        }

        /// <summary>
        /// Returns a switch as 2 cubic bézier curves.
        /// </summary>
        /// <param name="s">The switch prefab.</param>
        /// <param name="attachPoint">The attachment point.</param>
        /// <param name="handlePosition">The handle of the attachment point.</param>
        /// <param name="connectingPoint">Which point of the switch connects to the attachment point.</param>
        /// <returns>An array with 2 arrays representing the through track (index <c>0</c>) and diverging track (index <c>1</c>).</returns>
        public static Vector3[][] GetSwitchBeziers(Switch s, Vector3 attachPoint, Vector3 handlePosition, SwitchPoint connectingPoint)
        {
            // Create the original beziers.
            Vector3[][] curves = new Vector3[][]
            {
                new Vector3[]
                {
                    s.GetJointPoint().position,
                    s.GetJointPoint().globalHandle2,
                    s.GetThroughPoint().globalHandle1,
                    s.GetThroughPoint().position
                },
                new Vector3[]
                {
                    s.GetDivergeJoinPoint().position,
                    s.GetDivergeJoinPoint().globalHandle2,
                    s.GetDivergingPoint().globalHandle1,
                    s.GetDivergingPoint().position
                }
            };

            // Rotate and move the points to fit the target locations.
            Quaternion rotRoot;
            Quaternion rot;
            Vector3 pivot;

            if (connectingPoint == SwitchPoint.Joint)
            {
                rotRoot = Quaternion.identity;
                pivot = Vector3.zero;
            }
            else
            {
                BezierPoint bp;

                if (connectingPoint == SwitchPoint.Through)
                {
                    bp = s.GetThroughPoint();
                }
                else
                {
                    bp = s.GetDivergingPoint();
                }

                rotRoot = Quaternion.Inverse(Quaternion.LookRotation(bp.globalHandle1 - bp.position));
                pivot = bp.position;
            }

            rot = Quaternion.LookRotation(attachPoint - handlePosition);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < curves[i].Length; j++)
                {
                    curves[i][j] = (rot * (rotRoot * (curves[i][j] - pivot))) + attachPoint;
                }
            }

            return curves;
        }

        /// <summary>
        /// Calculates the radius of the diverging track of a <see cref="Switch"/>.
        /// </summary>
        public static float CalculateSwitchRadius(Switch s)
        {
            BezierPoint bp = s.GetDivergingPoint();
            return bp.position.z / Mathf.Sin(Mathf.Atan(Mathf.Abs(bp.handle1.x / bp.handle1.z)));
        }

        /// <summary>
        /// Calculates the angle at the end of the diverging track of a <see cref="Switch"/>.
        /// </summary>
        /// <returns>The angle in radians.</returns>
        public static float CalculateSwitchAngle(Switch s)
        {
            BezierPoint bp = s.GetDivergingPoint();
            return Mathf.Atan(Mathf.Abs(bp.handle1.x / bp.handle1.z));
        }

        /// <summary>
        /// Returns the speed limit shown on track speed signs for a given radius.
        /// </summary>
        /// <returns>The speed in km/h.</returns>
        public static float GetMaxSpeedForRadiusGame(float radius)
        {
            if (radius < 50f)
            {
                return 10f;
            }
            if (radius < 70f)
            {
                return 20f;
            }
            if (radius < 95f)
            {
                return 30f;
            }
            if (radius < 130f)
            {
                return 40f;
            }
            if (radius < 170f)
            {
                return 50f;
            }
            if (radius < 230f)
            {
                return 60f;
            }
            if (radius < 360f)
            {
                return 70f;
            }
            if (radius < 700f)
            {
                return 80f;
            }
            if (radius < 900f)
            {
                return 90f;
            }
            if (radius < 1200f)
            {
                return 100f;
            }
            return 120f;
        }

        /// <summary>
        /// Returns a closer estimate to the actual speed limit on tracks based on radius.
        /// </summary>
        /// <returns>The speed in km/h.</returns>
        public static float GetMaxSpeedForRadius(float radius)
        {
            // Based on certain ingame findings.
            // Here is some testing done after using the limits given by this function.
            // https://discord.com/channels/332511223536943105/560262776673796157/1136657491309105312
            // It's possible more testing is required, and in case the values are found
            // to be too high, then this older version should be used instead.
            //return 60.0f * Mathf.Sqrt(radius / 230.0f);
            return 60.0f * Mathf.Sqrt(radius / 170.0f);
        }

        public static string GetPrettyYardTrackName(Track t)
        {
            return $"[{t.yardId}{t.trackId}{t.trackType.LetterId()}]";
        }

        public static Vector3[] GetSmoothBezierToConnectSimple(Vector3 attachPosition, Vector3 attachHandle, Vector3 newTarget)
        {
            float length = (newTarget - attachPosition).magnitude * MathHelper.OneThird;
            Vector3 dir = (attachPosition - attachHandle).normalized;
            Vector3 handle = attachPosition + dir * length * 2;

            handle = newTarget + (handle - newTarget).normalized * length;

            return new Vector3[] { attachPosition,
                attachPosition + dir * length,
                handle,
                newTarget };
        }

        public static Vector3[] GetSmoothBezierToConnectSimple(Vector3 attachPosition, Vector3 attachHandle, Vector3 newTarget, float maxAngle)
        {
            Vector3 dir = (attachPosition - attachHandle).normalized;
            Vector3 dirNext = (newTarget - attachPosition).normalized;
            float angle = Vector3.Angle(dir, dirNext);

            if (angle > maxAngle)
            {
                dirNext = Quaternion.AngleAxis(maxAngle, Vector3.Cross(dir, dirNext)) * dir;
            }

            newTarget = Vector3.Project(newTarget - attachPosition, dirNext) + attachPosition;

            return GetSmoothBezierToConnectSimple(attachPosition, attachHandle, newTarget);
        }

        public static Vector3[] GetSmoothBezierToConnectComplex(Vector3 attachPosition, Vector3 attachHandle, Vector3 newTarget)
        {
            Vector3 dir = (attachPosition - attachHandle).normalized;
            Vector3 dirNext = (newTarget - attachPosition).normalized;
            Quaternion rot = Quaternion.FromToRotation(dir, dirNext);

            float length = (newTarget - attachPosition).magnitude * MathHelper.OneThird;
            bool sub360 = Vector3.Dot(dir, dirNext) < 0;

            dirNext = rot * dirNext;
            float angle = Vector3.Angle(dir, dirNext);

            if (sub360)
            {
                angle = 360 - angle;
            }

            length *= 1 + MathHelper.ArcToBezierHandleLength(angle * Mathf.Deg2Rad);

            return new Vector3[] { attachPosition,
                attachPosition + dir * length,
                newTarget - dirNext * length,
                newTarget };
        }

        public static Vector3[] GetSmoothBezierToConnectComplex(Vector3 attachPosition, Vector3 attachHandle, Vector3 newTarget, float maxAngle)
        {
            Vector3 dir = (attachPosition - attachHandle).normalized;
            Vector3 dirNext = (newTarget - attachPosition).normalized;
            float angle = Vector3.Angle(dir, dirNext);

            if (angle > maxAngle)
            {
                dirNext = Quaternion.AngleAxis(maxAngle, Vector3.Cross(dir, dirNext)) * dir;
            }

            newTarget = Vector3.Project(newTarget - attachPosition, dirNext) + attachPosition;

            return GetSmoothBezierToConnectComplex(attachPosition, attachHandle, newTarget);
        }

        public static Vector3[] GetSmoothBezierToConnectMix(Vector3 attachPosition, Vector3 attachHandle, Vector3 newTarget, float mix)
        {
            Vector3[] s = GetSmoothBezierToConnectSimple(attachPosition, attachHandle, newTarget);
            Vector3[] c = GetSmoothBezierToConnectComplex(attachPosition, attachHandle, newTarget);

            return new Vector3[] {Vector3.Lerp(s[0], c[0], mix),
                Vector3.Lerp(s[1], c[1], mix),
                Vector3.Lerp(s[2], c[2], mix),
                Vector3.Lerp(s[3], c[3], mix) };
        }

        public static Vector3[] GetSmoothBezierToConnectMix(Vector3 attachPosition, Vector3 attachHandle, Vector3 newTarget, float maxAngle, float mix)
        {
            Vector3[] s = GetSmoothBezierToConnectSimple(attachPosition, attachHandle, newTarget, maxAngle);
            Vector3[] c = GetSmoothBezierToConnectComplex(attachPosition, attachHandle, newTarget, maxAngle);

            return new Vector3[] {Vector3.Lerp(s[0], c[0], mix),
                Vector3.Lerp(s[1], c[1], mix),
                Vector3.Lerp(s[2], c[2], mix),
                Vector3.Lerp(s[3], c[3], mix) };
        }

        /// <summary>
        /// Returns all active <see cref="Track"/> objects.
        /// </summary>
        /// <remarks>
        /// This function is VERY slow, it is recommended to cache the result.
        /// </remarks>
        public static Track[] GetAllActiveTracks()
        {
            return Object.FindObjectsOfType<Track>();
        }

        /// <summary>
        /// Searches <see cref="CachedTracks"/> for a possible snap point.
        /// </summary>
        /// <param name="point">The point to snap.</param>
        /// <param name="snappedPosition">The position where to snap.</param>
        /// <param name="snappedHandle">The handle of the snap.</param>
        /// <returns>True if there was a snap.</returns>
        /// <remarks>
        /// Both <paramref name="snappedPosition"/> and <paramref name="snappedHandle"/> will be equal to
        /// <paramref name="point"/> in case no snap position is found.
        /// </remarks>
        public static bool CheckForTrackSnap(Vector3 point, float radius, out Vector3 snappedPosition, out Vector3 snappedHandle)
        {
            BezierCurve here;
            radius *= radius;

            for (int i = 0; i < CachedTracks.Length; i++)
            {
                here = CachedTracks[i].Curve;

                if ((point - here[0].position).sqrMagnitude <= radius)
                {
                    snappedPosition = here[0].position;
                    snappedHandle = here[0].globalHandle2;
                    return true;
                }

                if ((point - here.Last().position).sqrMagnitude <= radius)
                {
                    snappedPosition = here.Last().position;
                    snappedHandle = here.Last().globalHandle1;
                    return true;
                }
            }

            snappedPosition = point;
            snappedHandle = point;
            return false;
        }

        /// <summary>
        /// Reverses a cubic bezier represented by its 4 control points.
        /// </summary>
        public static Vector3[] ReverseCurve(Vector3[] curve)
        {
            return new Vector3[] { curve[3],
                curve[2],
                curve[1],
                curve[0] };
        }

        /// <summary>
        /// Adjusts the handle of a bezier curve vertically to match a normal at a position.
        /// </summary>
        /// <param name="position">The point of the curve.</param>
        /// <param name="handle">The handle at that point.</param>
        /// <param name="normal">The normal at that point.</param>
        /// <returns>The modified handle.</returns>
        /// <remarks>In case the normal is not vertical enough, the handle is not changed.</remarks>
        public static Vector3 HandleMatchNormal(Vector3 position, Vector3 handle, Vector3 normal)
        {
            // The normal not being somewhat vertical can cause weird values, so don't change in these cases.
            if (Mathf.Approximately(normal.y, 0))
            {
                return handle;
            }

            Vector3 dir = handle - position;
            normal = Vector3.ProjectOnPlane(normal, Vector3.Cross(dir, Vector3.up));
            dir.y = 0;
            dir.y = dir.magnitude * MathHelper.GetGrade(Vector3.Angle(normal, Vector3.up));

            if (Vector3.Dot(normal, dir) > 0)
            {
                dir.y = -dir.y;
            }

            return position + dir;
        }

        #region INTERNAL

        // Copies a track's fields to another.
        internal static void CopyTrackFields(Track original, Track other)
        {
            other.age = original.age;
            other.generateBallast = original.generateBallast;
            other.generateSigns = original.generateSigns;
            other.generateSleepers = original.generateSleepers;
            other.stationId = original.stationId;
            other.trackId = original.trackId;
            other.trackType = original.trackType;
            other.yardId = original.yardId;
        }

        // The length of the straight section (middle track) of a crossover.
        internal static float CalculateCrossoverDistance(Switch switchPrefab, float trackDistance)
        {
            float targetDistance = trackDistance - (2.0f * Mathf.Abs(switchPrefab.DivergingTrack.Curve[1].position.x));
            Vector3 handle = switchPrefab.DivergingTrack.Curve[1].handle1;

            return (targetDistance / Mathf.Abs(handle.x)) * handle.magnitude;
        }

        // The length of the straight section connecting the middle switch to the outer switches in a yard.
        internal static float CalculateLengthFromDistanceYardCentre(Switch switchPrefab, float trackDistance)
        {
            float targetDistance = trackDistance - Mathf.Abs(switchPrefab.DivergingTrack.Curve[1].position.x);

            Vector3 point = switchPrefab.DivergingTrack.Curve[1].position;
            Vector3 handle = switchPrefab.DivergingTrack.Curve[1].handle1;
            Vector3 proj = Vector3.Project(point, handle);
            targetDistance -= (proj - point).magnitude;

            return (targetDistance / Mathf.Abs(handle.x)) * handle.magnitude;
        }

        // The length of the straight section connecting the outer switches in a yard.
        internal static float CalculateLengthFromDistanceYardSides(Switch switchPrefab, float trackDistance)
        {
            float targetDistance = trackDistance;

            Vector3 point = switchPrefab.ThroughTrack.Curve[1].position;
            Vector3 handle = switchPrefab.DivergingTrack.Curve[1].handle1;
            Vector3 proj = Vector3.Project(point, handle);
            targetDistance -= (proj - point).magnitude;

            return (targetDistance / handle.x) * handle.magnitude;
        }

        // The length of the track between the middle yard switches.
        internal static float CalculateYardMidSwitchDistance(float trackDistance)
        {
            return trackDistance * 2.0f;
        }

        #endregion

        #region EDITOR ONLY
#if UNITY_EDITOR

        /// <summary>
        /// Tries to assign the default prefabs in case a value is null.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="buffer"></param>
        /// <param name="switchLeft"></param>
        /// <param name="switchRight"></param>
        /// <param name="turntable"></param>
        /// <remarks>
        /// This will only look in the default directory (Mapify folder in the Assets root).
        /// If a parameter is not null, it will not be replaced.
        /// </remarks>
        public static void TryGetDefaultPrefabs(ref Track track, ref BufferStop buffer, ref Switch switchLeft, ref Switch switchRight, ref Turntable turntable)
        {
            string[] guids;

            if (track == null)
            {
                guids = AssetDatabase.FindAssets("Track", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    track = AssetDatabase.LoadAssetAtPath<Track>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (buffer == null)
            {
                guids = AssetDatabase.FindAssets("Buffer Stop", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    buffer = AssetDatabase.LoadAssetAtPath<BufferStop>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (switchLeft == null)
            {
                guids = AssetDatabase.FindAssets("Switch Left", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    switchLeft = AssetDatabase.LoadAssetAtPath<Switch>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (switchRight == null)
            {
                guids = AssetDatabase.FindAssets("Switch Right", new[] { "Assets/Mapify/Prefabs/Trackage" });

                if (guids.Length > 0)
                {
                    switchRight = AssetDatabase.LoadAssetAtPath<Switch>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (turntable == null)
            {
                guids = AssetDatabase.FindAssets("Turntable", new[] { "Assets/Mapify/Prefabs/Trackage" });

                for (int i = 0; i < guids.Length; i++)
                {
                    var turn = AssetDatabase.LoadAssetAtPath<Turntable>(AssetDatabase.GUIDToAssetPath(guids[i]));

                    if (turn != null)
                    {
                        turntable = turn;
                        break;
                    }
                }
            }
        }

#endif
        #endregion
    }
}
