using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Mapify.Editor.Utils
{
    public static class Extensions
    {
        #region GameObjects & Components

        public static float DistToSceneCamera(this Transform t)
        {
#if UNITY_EDITOR
            return (t.position - Camera.current.transform.position).sqrMagnitude;
#else
            throw new InvalidOperationException($"{nameof(Extensions)}.{nameof(DistToSceneCamera)} can only be used in the editor");
#endif
        }

        public static T GetComponentInSelfOrParent<T>(this Component component)
        {
            T self = component.GetComponent<T>();
            return self != null ? self : component.GetComponentInParent<T>();
        }

        public static T[] GetFirstComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive)
                .GroupBy(c => c.gameObject)
                .Select(g => g.First())
                .ToArray();
        }

        public static T GetClosestComponent<T>(this GameObject gameObject) where T : Component
        {
            return Object.FindObjectsOfType<T>()
                .OrderBy(c => (gameObject.transform.position - c.transform.position).sqrMagnitude)
                .FirstOrDefault();
        }

        public static GameObject FindChildByName(this GameObject parent, string name)
        {
            Transform child = FindChildByName(parent.transform, name);
            return child == null ? null : child.gameObject;
        }

        public static GameObject[] FindChildrenByName(this GameObject parent, string name)
        {
            return parent.transform.FindChildrenByName(name).Select(t => t.gameObject).ToArray();
        }

        public static Transform FindChildByName(this Transform parent, string name)
        {
            return FindChildrenByName(parent, name).FirstOrDefault();
        }

        public static Transform[] FindChildrenByName(this Transform parent, string name)
        {
            return parent.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToArray();
        }

        public static Transform[] GetChildren(this Transform parent)
        {
            Transform[] children = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++) children[i] = parent.GetChild(i);
            return children;
        }

        public static T[] GetAllComponents<T>(this Scene scene, bool includeInactive = false)
        {
            return scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(includeInactive)).ToArray();
        }

        public static GameObject[] GetAllGameObjects(this Scene scene)
        {
            return scene.GetAllComponents<Transform>().Select(t => t.gameObject).ToArray();
        }

        public static void RecordObjectChanges(this IEnumerable<Object> objects, Action func)
        {
#if UNITY_EDITOR
            Object[] nonNullObjects = objects.Where(obj => obj != null).ToArray();
            Undo.IncrementCurrentGroup();
            Undo.RecordObjects(nonNullObjects, "Object Changes");

            func.Invoke();

            foreach (Object o in nonNullObjects.Where(PrefabUtility.IsPartOfPrefabInstance))
                PrefabUtility.RecordPrefabInstancePropertyModifications(o);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.SaveOpenScenes();
#endif
        }

        public static float CalculateWorldSize(this IEnumerable<Terrain> terrains)
        {
            float maxX = 0f;
            float maxZ = 0f;

            foreach (Terrain terrain in terrains)
            {
                // This is validated in the TerrainValidator, but can be called elsewhere, so just ignore it.
                if (terrain.terrainData == null)
                    continue;
                Vector3 terrainSize = terrain.terrainData.size;
                Vector3 position = terrain.transform.position;
                float terrainMaxX = position.x + terrainSize.x;
                float terrainMaxZ = position.z + terrainSize.z;
                if (terrainMaxX > maxX) maxX = terrainMaxX;
                if (terrainMaxZ > maxZ) maxZ = terrainMaxZ;
            }

            return Mathf.Max(maxX, maxZ);
        }

        public static void ReparentAllChildren(this Transform transform, Transform newParent)
        {
            List<Transform> children = new List<Transform>();

            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            foreach (Transform child in children)
            {
                child.parent = newParent;
            }
        }

        #endregion

        #region Misc. Unity Types

        public static Terrain[] Sort(this Terrain[] terrains)
        {
            return terrains.OrderBy(go =>
            {
                Vector3 pos = go.transform.position;
                return pos.z * terrains.Length + pos.x;
            }).ToArray();
        }

        public static (float, float, float, float) GroupedBounds(this IEnumerable<Renderer> renderers)
        {
            Bounds[] allBounds = renderers.Select(r => r.bounds).ToArray();
            float minX = allBounds.Min(b => b.min.x);
            float minZ = allBounds.Min(b => b.min.z);
            float maxX = allBounds.Max(b => b.max.x);
            float maxZ = allBounds.Max(b => b.max.z);
            return (minX, minZ, maxX, maxZ);
        }

        public static Vector2 Flatten(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        public static Vector3 Flatten3D(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }

        public static Vector3 To3D(this Vector2 vector, float y)
        {
            return new Vector3(vector.x, y, vector.y);
        }

        /// <param name="alpha">If the alpha value should be inverted or not.</param>
        public static Color Negative(this Color color, bool alpha = false)
        {
            return new Color(
                1.0f - color.r,
                1.0f - color.g,
                1.0f - color.b,
                alpha ? 1.0f - color.a : color.a);
        }

        public static float HorizontalMagnitude(this Vector3 vector)
        {
            vector.y = 0;
            return vector.magnitude;
        }

        #endregion

        #region Bezier Curves

        public static BezierCurve Curve(this BezierPoint point)
        {
            if (point._curve != null) return point._curve;
            BezierCurve curve = point.GetComponentInParent<BezierCurve>();
            point._curve = curve;
            return curve;
        }

        public static BezierPoint[] GetFirstAndLastPoints(this BezierCurve curve)
        {
            return new[] { curve[0], curve.Last() };
        }

        public static Vector3[] AsControlPoints(this BezierCurve curve, int from)
        {
            return new Vector3[] { curve[from].position,
                curve[from].globalHandle2,
                curve[from + 1].globalHandle1,
                curve[from + 1].position};
        }

        #endregion

        #region C# Utils

        public static List<T> ToList<T>(this IEnumerator<T> e)
        {
            List<T> list = new List<T>();
            while (e.MoveNext()) list.Add(e.Current);
            return list;
        }

        #endregion

        #region Mapify

        public static string PrettySceneName(this string sceneName)
        {
            return sceneName.Split('/').Last().Split('.').First();
        }

        public static Dictionary<Station, List<T>> MapToClosestStation<T>(this IEnumerable<T> arr) where T : Component
        {
            return arr
                .GroupBy(spawner => spawner.gameObject.GetClosestComponent<Station>())
                .Where(group => group.Key != null)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        // Track.
        /// <summary>
        /// Returns true if this track starts at grade of 0%.
        /// </summary>
        public static bool IsStartLevel(this Track track)
        {
            return Mathf.Approximately(track.Curve[0].position.y, track.Curve[0].globalHandle2.y);
        }

        /// <summary>
        /// Returns true if this track ends at grade of 0%.
        /// </summary>
        public static bool IsEndLevel(this Track track)
        {
            return Mathf.Approximately(track.Curve.Last().position.y, track.Curve.Last().globalHandle1.y);
        }

        /// <summary>
        /// Returns the grade at the start of this track.
        /// </summary>
        public static float GetGradeAtStart(this Track track)
        {
            return MathHelper.GetGrade(track.Curve[0].position, track.Curve[0].globalHandle2);
        }

        /// <summary>
        /// Returns the grade at the end of this track.
        /// </summary>
        public static float GetGradeAtEnd(this Track track)
        {
            return MathHelper.GetGrade(track.Curve.Last().globalHandle1, track.Curve.Last().position);
        }

        /// <summary>
        /// The height difference between the starting point and ending point.
        /// </summary>
        public static float GetHeightChange(this Track track)
        {
            return track.Curve.Last().position.y - track.Curve[0].position.y;
        }

        public static float GetHorizontalLength(this Track track, float resolution = 0.5f)
        {
            float length = 0;
            BezierCurve curve = track.Curve;

            for (int i = 1; i < curve.pointCount; i++)
            {
                length += BezierCurve.ApproximateLength(
                    curve[i - 1].position.Flatten3D(),
                    curve[i - 1].globalHandle2.Flatten3D(),
                    curve[i].position.Flatten3D(),
                    curve[i].globalHandle1.Flatten3D(), resolution);
            }

            return length;
        }

        public static float GetAverageGrade(this Track track, float resolution = 0.5f)
        {
            return track.GetHeightChange() / track.GetHorizontalLength(resolution);
        }

        // BezierCurve
        /// <summary>
        /// Returns true if a this curve does not change height at any point.
        /// </summary>
        public static bool IsCompletelyLevel(this BezierCurve curve)
        {
            float y = curve[0].position.y;
            int count = curve.pointCount;

            for (int i = 1; i < count; i++)
            {
                if (!Mathf.Approximately(curve[i].position.y, y) ||
                    !Mathf.Approximately(curve[i].globalHandle1.y, y) ||
                    !Mathf.Approximately(curve[i - 1].globalHandle2.y, y))
                {
                    return false;
                }
            }

            return true;
        }

        public static Vector3[] GetAllPoints(this BezierCurve curve)
        {
            Vector3[] points = new Vector3[curve.pointCount * 3];

            for (int i = 0; i < curve.pointCount; i++)
            {
                points[(i * 3)] = curve[i].globalHandle1;
                points[(i * 3) + 1] = curve[i].position;
                points[(i * 3) + 2] = curve[i].globalHandle2;
            }

            return points;
        }

        // BezierPoint.
        /// <summary>
        /// Returns the grade for the next handle.
        /// </summary>
        public static float GetGradeForwards(this BezierPoint bp)
        {
            return MathHelper.GetGrade(bp.position, bp.globalHandle2);
        }

        /// <summary>
        /// Returns the grade for the rear handle.
        /// </summary>
        public static float GetGradeBackwards(this BezierPoint bp)
        {
            return MathHelper.GetGrade(bp.globalHandle1, bp.position);
        }

        #endregion
    }
}
