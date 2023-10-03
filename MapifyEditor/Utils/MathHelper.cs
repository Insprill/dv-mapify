using System;
using UnityEngine;

namespace Mapify.Editor.Utils
{
    public static class MathHelper
    {
        public const float OneThird = 1.0f / 3.0f;
        public const float TwoThirds = 2.0f / 3.0f;
        public const float FourThirds = 4.0f / 3.0f;
        /// <summary>2π (6.28318...)</summary>
        public const float Tau = (float)(Math.PI * 2.0);

        private static Plane _yPlane = new Plane(Vector3.up, 0);

        /// <summary>The plane <c>Y=0</c>.</summary>
        public static Plane YPlane => _yPlane;

        /// <summary>Clamps a float to the range [-180, 180].</summary>
        public static float ClampAngle(float value)
        {
            return Mathf.Clamp(value, -180.0f, 180.0f);
        }

        /// <summary>Then handle length for a cubic bézier curve that approximates an arc.</summary>
        public static float ArcToBezierHandleLength(float arc)
        {
            return FourThirds * Mathf.Tan(arc * 0.25f);
        }

        public static Vector2 Rotate(Vector2 v, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        public static Vector2 RotateAround(Vector2 v, float angle, Vector2 pivot)
        {
            return Rotate(v - pivot, angle) + pivot;
        }

        public static Vector2 RotateCW(Vector2 v)
        {
            return new Vector2(v.y, -v.x);
        }

        public static Vector2 RotateCCW(Vector2 v)
        {
            return new Vector2(-v.y, v.x);
        }

        public static float GetGrade(float angle)
        {
            return Mathf.Tan(angle * Mathf.Deg2Rad);
        }

        public static float GetGrade(Vector3 p0, Vector3 p1)
        {
            float height = p1.y - p0.y;

            p0.y = 0;
            p1.y = 0;

            return height / (p1 - p0).magnitude;
        }

        public static float AngleToNorth(Vector3 dir)
        {
            dir.y = 0;

            return Vector3.Angle(dir, Vector3.forward);
        }

        public static Vector3 MirrorAround(Vector3 v, Vector3 mirrorPoint)
        {
            return mirrorPoint + (mirrorPoint - v);
        }

        // https://www.geeksforgeeks.org/program-for-point-of-intersection-of-two-lines/
        public static Vector2 LineLineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            // Line AB represented as a1x + b1y = c1
            float a1 = b.y - a.y;
            float b1 = a.x - b.x;
            float c1 = a1 * (a.x) + b1 * (a.y);

            // Line CD represented as a2x + b2y = c2
            float a2 = d.y - c.y;
            float b2 = c.x - d.x;
            float c2 = a2 * (c.x) + b2 * (c.y);

            float determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                // The lines are parallel. This is simplified
                // by returning a pair of FLT_MAX
                return new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            }

            float x = (b2 * c1 - b1 * c2) / determinant;
            float y = (a1 * c2 - a2 * c1) / determinant;
            return new Vector2(x, y);
        }

        public static Vector3[] SampleCircle(Vector3 center, float radius, int samples = 32)
        {
            Vector3[] result = new Vector3[samples + 1];
            float angle;
            float sin;
            float cos;

            for (int i = 0; i < samples; i++)
            {
                angle = (Tau * i) / samples;
                sin = Mathf.Sin(angle);
                cos = Mathf.Cos(angle);
                result[i] = center + new Vector3(sin, 0, cos) * radius;
            }

            result[samples] = result[0];

            return result;
        }

        public static Vector3[] SampleBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples = 8)
        {
            Vector3[] results = new Vector3[samples];
            float f;

            for (int i = 0; i < samples; i++)
            {
                f = i / (float)(samples - 1);

                results[i] = Vector3.Lerp(
                    Vector3.Lerp(
                        Vector3.Lerp(p0, p1, f),
                        Vector3.Lerp(p1, p2, f), f),
                    Vector3.Lerp(
                        Vector3.Lerp(p1, p2, f),
                        Vector3.Lerp(p2, p3, f), f), f);
            }

            return results;
        }

        /// <summary>
        /// Assumes a length of 4.
        /// </summary>
        public static Vector3[] SampleBezier(Vector3[] curve, int samples = 8)
        {
            return SampleBezier(curve[0], curve[1], curve[2], curve[3], samples);
        }

        public static Vector3[][] SampleBeziers(Vector3[][] curves, int samples = 8)
        {
            Vector3[][] results = new Vector3[curves.Length][];

            for (int i = 0; i < curves.Length; i++)
            {
                results[i] = SampleBezier(curves[i], samples);
            }

            return results;
        }

        /// <summary>
        /// Splits a bezier curve into 2 at a point.
        /// </summary>
        /// <param name="curve">The 4 points of a cubic bezier.</param>
        /// <param name="f">The point in the curve where it is split.</param>
        /// <returns></returns>
        public static Vector3[][] SplitBezier(Vector3[] curve, float f)
        {
            Vector3[][] results = new Vector3[2][];
            results[0] = new Vector3[4];
            results[1] = new Vector3[4];

            Vector3 mid = curve[1] + (curve[2] - curve[1]) * f;

            results[0][0] = curve[0];
            results[0][1] = curve[0] + (curve[1] - curve[0]) * f;
            results[0][2] = results[0][1] + (mid - results[0][1]) * f;
            results[0][3] = BezierCurve.GetCubicCurvePoint(curve[0], curve[1], curve[2], curve[3], f);

            results[1][3] = curve[3];
            results[1][2] = curve[3] + (curve[2] - curve[3]) * f;
            results[1][1] = results[1][2] + (mid - results[1][2]) * f;
            results[1][0] = results[0][3];

            return results;
        }
    }
}
