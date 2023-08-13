using Mapify.Editor.Utils;
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
    }
}
